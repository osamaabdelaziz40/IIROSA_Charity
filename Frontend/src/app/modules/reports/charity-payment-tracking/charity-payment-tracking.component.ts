import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { ReportPdfService, ReportPreviewSource, ReportSheetConfig } from '../services/report-pdf.service';
import { CharityService } from '../../charities/services/charity.service';
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CharityPaymentTrackingRow, FamilyUpdateTrackingRow } from '../models/report.model';

/**
 * UC-RPT-20 (§23.S.12 متابعة الجمعيات) — the cross-charity tracking grid over one payment
 * batch: one row per charity present in the batch (members, entered reports, upload
 * state). متابعة تسليمات الجميعات runs the query. متابعة تحديثات الجميعات/تم are the
 * family-update sheet commands (UC-RPT-21, wired here): the first loads the sheet data,
 * تم prints it through the epic-wide browser-print path (report-pdf.service). غلق resets
 * the panel. HQ-only — the endpoint authorises; this screen's guards are convenience.
 */
@Component({
  selector: 'app-charity-payment-tracking',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './charity-payment-tracking.component.html',
  styleUrls: ['./charity-payment-tracking.component.scss']
})
export class CharityPaymentTrackingComponent implements OnInit, OnDestroy {
  rows: CharityPaymentTrackingRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** HQ-only screen — the charity narrow is an HQ dropdown, hidden for charity callers. */
  isHQ = false;

  /** The export pages through the whole grid — this endpoint's validator caps PageSize at 100. */
  private readonly exportPageSize = 100;

  charityOptions: Array<{ id: string; name: string }> = [];
  batchOptions: Array<{ id: string; name: string }> = [];

  /** UC-RPT-21 sheet state — loaded by متابعة تحديثات الجميعات, printed by تم. */
  updateRows: FamilyUpdateTrackingRow[] = [];
  updateTotal = 0;
  updateLoaded = false;
  updateLoading = false;

  /** One print sheet is one page-set — the endpoint's validator caps PageSize at 500. */
  private readonly sheetPageSize = 500;

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private reportPdfService: ReportPdfService,
    private charityService: CharityService,
    private orphanPaymentService: OrphanPaymentService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      batchId: [''],
      updateDate: ['']
    });
  }

  ngOnInit(): void {
    // Review P27 2026-08-26: the screen's own doc says HQ-only — the charity narrow now
    // actually gates on the HQ roles (the endpoint authorises regardless; the previously
    // injected authService was dead weight).
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
    // The batch picker follows the charity narrow (10-6 pattern) — an absent batch means
    // the current one (the latest payment group by GroupDate).
    this.filterForm.get('charityId')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadBatches());
    this.loadBatches();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — real charities endpoint only; كل الجهات all-option (HQ-only screen). */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.charityTracking.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading charities')
      });
  }

  /** الدفعة — distinct batch numbers in scope (18-1's reuse rule). */
  private loadBatches(): void {
    const charityNarrow = this.filterForm.get('charityId')!.value || undefined;
    this.orphanPaymentService.getBatchNumbers(charityNarrow)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.batchOptions = [
            { id: '', name: this.translate.instant('reports.charityTracking.currentBatch') },
            ...(options || []).map(b => ({ id: b.batchNo, name: b.batchNo }))
          ];
        },
        error: () => console.error('Error loading batch numbers')
      });
  }

  // ==================== commands (§23.S.12) ====================

  /** متابعة تسليمات الجميعات — runs the tracking grid. A read; nothing stored changes. */
  runTracking(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.currentPage = 1;
    this.runSearch();
  }

  onPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  /** غلق — the legacy modal-close handler flattened to an inline reset. */
  resetFilters(): void {
    this.filterForm.reset({ charityId: '', batchId: '', updateDate: '' });
    this.rows = [];
    this.totalCount = 0;
    this.hasRun = false;
    this.currentPage = 1;
    this.updateRows = [];
    this.updateTotal = 0;
    this.updateLoaded = false;
  }

  // ==================== UC-RPT-21 family-update sheet (18-21) ====================

  /**
   * متابعة تحديثات الجميعات — loads the family-update sheet data: families of the anchor
   * payment's charities refreshed from the requested date. The date is the one required
   * input (من فضلك ادخل تاريخ بدا التحديث): missing → flag the picker, no call.
   */
  loadUpdates(): void {
    if (this.loading || this.exporting || this.updateLoading) {
      return;
    }

    const dateControl = this.filterForm.get('updateDate')!;
    const date = String(dateControl.value || '').trim();
    if (!date) {
      // Review P27 2026-08-26: merge, never clobber — a blanket setErrors also wiped a
      // coexisting server reason off the control from a previous failed submit.
      dateControl.setErrors({ ...dateControl.errors, required: true });
      this.notification.info(this.translate.instant('reports.charityTracking.dateRequired'));
      return;
    }
    // Review P27 2026-08-26: clear ONLY the required flag — a server reason survives.
    const merged = { ...dateControl.errors };
    delete merged['required'];
    dateControl.setErrors(Object.keys(merged).length ? merged : null);

    const v = this.filterForm.getRawValue();
    const filter = {
      page: 1,
      pageSize: this.sheetPageSize,
      date,
      batchNo: v.batchId ? String(v.batchId) : undefined,
      charityId: v.charityId ? String(v.charityId) : undefined
    };

    this.updateLoading = true;
    this.reportService.getFamilyUpdateTracking(filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.updateRows = result.items || [];
          this.updateTotal = result.totalCount || 0;
          this.updateLoaded = true;
          this.updateLoading = false;
          // Review P22 2026-08-26: the updates sheet loads ONE 500-capped page — when the
          // selection exceeds it, the sheet (and its print) is partial and says so.
          if ((result.totalCount || 0) > (result.items || []).length) {
            this.notification.warning(this.translate.instant('reports.print.truncated'));
          }
          this.notification.info(
            this.translate.instant('reports.charityTracking.updatesLoaded', { count: this.updateTotal })
          );
        },
        error: (httpError: any) => {
          this.updateLoading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.charityTracking.loadUpdatesFailed')
          );
        }
      });
  }

  /**
   * تم — prints the family-update sheet via the epic-wide browser-print path. Loads the
   * data first if متابعة تحديثات الجميعات hasn't run; an empty result prints nothing —
   * the nothing-to-produce message replaces a header-only sheet.
   */
  printUpdates(): void {
    if (this.loading || this.exporting || this.updateLoading) {
      return;
    }
    if (!this.updateLoaded) {
      this.loadUpdates();
      return;
    }
    if (this.updateTotal === 0 || this.updateRows.length === 0) {
      this.notification.info(this.translate.instant('reports.charityTracking.nothingToPrint'));
      return;
    }

    try {
      this.reportPdfService.printSheet(this.buildUpdatesSheetConfig());
    } catch {
      this.notification.error(this.translate.instant('reports.charityTracking.printFailed'));
    }
  }

  /**
   * §23.U.40 عرض — the updates sheet over the loaded updates set (the print command loads it
   * on demand first; عرض is available once it is loaded — honest absence until then).
   */
  previewDocument = (): ReportPreviewSource | null  => {
    return this.updateLoaded && this.updateRows.length
      ? this.reportPdfService.sheetSource(this.buildUpdatesSheetConfig())
      : null;
  }

  /** The updates sheet's config — shared by the print command and the §23.U.40 preview. */
  private buildUpdatesSheetConfig(): ReportSheetConfig<FamilyUpdateTrackingRow> {
    const v = this.filterForm.getRawValue();
    return {
        documentTitle: `family-update-tracking_${String(v.updateDate || '').slice(0, 10) || 'sheet'}`,
        title: this.translate.instant('reports.familyUpdate.sheetTitle'),
        subtitle: this.translate.instant('reports.charityTracking.subtitle'),
        meta: [
          { label: this.translate.instant('reports.charityTracking.filterBatch'), value: String(v.batchId || '') || this.translate.instant('reports.charityTracking.currentBatch') },
          { label: this.translate.instant('reports.charityTracking.filterUpdateDate'), value: String(v.updateDate || '').slice(0, 10) }
        ],
        columns: [
          { header: this.translate.instant('reports.familyUpdate.colSerial'), render: (_row, index) => String(index + 1) },
          { header: this.translate.instant('reports.familyUpdate.colFamilyCode'), render: row => row.familyCode || '—' },
          { header: this.translate.instant('reports.familyUpdate.colHeadOfFamily'), render: row => row.headOfFamily || '—' },
          { header: this.translate.instant('reports.familyUpdate.colCharity'), render: row => row.charityName || '—' },
          { header: this.translate.instant('reports.familyUpdate.colUpdatedOn'), render: row => this.formatDate(row.updatedOn) }
        ],
        rows: this.updateRows
    };
  }

  /** Sheet date cells — dd/MM/yyyy (the grid's pipe format). */
  private formatDate(iso?: string | null): string {
    if (!iso) {
      return '—';
    }
    const d = new Date(iso);
    if (isNaN(d.getTime())) {
      return '—';
    }
    const pad = (n: number): string => String(n).padStart(2, '0');
    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`;
  }

  private runSearch(): void {
    this.loading = true;
    this.reportService.getCharityPaymentTracking(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.charityTracking.searchFailed')
          );
        }
      });
  }

  /** استخراج البيانات — the tracking sheet over the WHOLE grid, paging under the 100 cap. */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (this.totalCount === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.exporting = true;
    const collected: CharityPaymentTrackingRow[] = [];
    const totalPages = Math.ceil(this.totalCount / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        const v = this.filterForm.getRawValue();
        const scope = v.charityId ? String(v.charityId).slice(0, 8) : 'all';
        this.reportExportService.exportCharityPaymentTracking(
          collected,
          `charity-payment-tracking_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.charityTracking.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getCharityPaymentTracking(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.charityTracking.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): { page: number; pageSize: number; charityId?: string; batchId?: string; dateOfStartingUpdate?: string } {
    const v = this.filterForm.getRawValue();
    const filter: { page: number; pageSize: number; charityId?: string; batchId?: string; dateOfStartingUpdate?: string } = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.batchId) {
      filter.batchId = String(v.batchId);
    }
    if (v.updateDate) {
      filter.dateOfStartingUpdate = String(v.updateDate);
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: CharityPaymentTrackingRow): string {
    return row.charityId;
  }
}
