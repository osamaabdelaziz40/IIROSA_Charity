import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, firstValueFrom } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { ReportPdfService, ReportPreviewSource, ReportCardSheetConfig } from '../services/report-pdf.service';
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { CharityService } from '../../charities/services/charity.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanDataFilter, OrphanDataRow, ReceiptCardRow, ReceiptCardsReport } from '../models/report.model';

/**
 * UC-RPT-01 (§23.S.3 بيانات الأيتام) — the caller-scoped orphan master listing.
 *
 * Thin by design: the shared report-viewer shell owns the chrome (بحث / استخراج البيانات /
 * Pagination / empty states); this component owns the §23.S.3 filter panel (batch, charity for
 * HQ, governorate→center cascade, age range, the three mutually-exclusive orphan flags) and the
 * bespoke ~24-column grid subset. The Excel export carries the full column contract and pages
 * through the whole selection server-side (PageSize cap 200, page by page).
 *
 * The charity scope is enforced SERVER-SIDE — the charity filter narrows HQ queries only;
 * hiding it for charity users is convenience, not the control.
 */
@Component({
  selector: 'app-orphan-data-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './orphan-data-report.component.html',
  styleUrls: ['./orphan-data-report.component.scss']
})
export class OrphanDataReportComponent implements OnInit, OnDestroy {
  rows: OrphanDataRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  // 18-31 (UC-RPT-31): the receipt-cards command — true while its request is in flight.
  cardsPrinting = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole selection — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  isHQ = false;

  batchOptions: Array<{ id: string; name: string }> = [];
  charityOptions: Array<{ id: string; name: string }> = [];
  governorateOptions: Array<{ id: number; name: string }> = [];
  centerOptions: Array<{ id: number; name: string }> = [];
  centersDisabled = true;

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private reportPdfService: ReportPdfService,
    private orphanPaymentService: OrphanPaymentService,
    private charityService: CharityService,
    private lookupService: LookupManagementService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      batchNumber: [''],
      charityId: [''],
      governorateId: [null],
      centerId: [null],
      ageFrom: [null],
      ageTo: [null],
      // العمر checkbox — accepted; finished-sponsorship semantics land with 18-4
      isFinishedSponsorship: [false],
      notExcluded: [true],
      excluded: [false],
      allOrphans: [false]
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    this.loadBatches();
    if (this.isHQ) {
      this.loadCharities();
    }
    this.loadGovernorates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== lookups ====================

  /** الدفعة المالية المنصرقة للايتام — GET /api/OrphanPayments/batch-numbers. */
  private loadBatches(): void {
    this.orphanPaymentService.getBatchNumbers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.batchOptions = [
            { id: '', name: this.translate.instant('reports.orphanData.allBatches') },
            ...(options || []).map(o => ({ id: o.batchNo, name: o.batchNo }))
          ];
        },
        error: () => console.error('Error loading batch numbers')
      });
  }

  /** الجمعية — HQ only; a charity caller is scoped server-side regardless. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.orphanData.allCharities') },
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

  /** المحافظة — GET /api/LookupManagement/regions. */
  private loadGovernorates(): void {
    this.lookupService.getRegions({ page: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.governorateOptions = [
            { id: 0, name: this.translate.instant('common.all') },
            ...(response.items || []).map(r => ({ id: r.id, name: r.nameAr || r.nameEn || r.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading governorates')
      });
  }

  /** المركز — reloaded on every governorate change; disabled until one is chosen. */
  private loadCenters(regionId: number): void {
    this.lookupService.getCentersByRegion(regionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: centers => {
          this.centerOptions = [
            { id: 0, name: this.translate.instant('common.all') },
            ...(centers || []).map(c => ({ id: c.id, name: c.nameAr || c.nameEn || c.name }))
          ];
          this.centersDisabled = false;
        },
        error: () => {
          console.error('Error loading centers');
          this.centersDisabled = true;
        }
      });
  }

  /** Governorate change → reset the center selection and reload its list. */
  onGovernorateChange(): void {
    this.filterForm.get('centerId')?.reset(null);
    const regionId = Number(this.filterForm.get('governorateId')?.value);
    if (regionId > 0) {
      this.centerOptions = [{ id: 0, name: this.translate.instant('common.all') }];
      this.loadCenters(regionId);
    } else {
      this.centerOptions = [];
      this.centersDisabled = true;
    }
  }

  // ==================== orphan flags (§23.S.3 mutual exclusion) ====================

  /** المستبعدين change → DisableAllOrphan() — the legacy handler name kept in spirit. */
  onExcludedChange(): void {
    if (this.filterForm.get('excluded')?.value) {
      this.filterForm.patchValue({ allOrphans: false, notExcluded: false }, { emitEvent: false });
      this.filterForm.get('allOrphans')?.disable({ emitEvent: false });
    } else {
      this.filterForm.get('allOrphans')?.enable({ emitEvent: false });
      this.filterForm.patchValue({ notExcluded: true }, { emitEvent: false });
    }
  }

  /** All-orphans change → DisableExcluded(). */
  onAllOrphansChange(): void {
    if (this.filterForm.get('allOrphans')?.value) {
      this.filterForm.patchValue({ excluded: false, notExcluded: false }, { emitEvent: false });
      this.filterForm.get('excluded')?.disable({ emitEvent: false });
    } else {
      this.filterForm.get('excluded')?.enable({ emitEvent: false });
      this.filterForm.patchValue({ notExcluded: true }, { emitEvent: false });
    }
  }

  // ==================== receipt cards (18-31 §23.U.31) ====================

  /**
   * كروت التسليم — the batch's collection cards, one per (guardian, orphan) payment item.
   * The screen's batch dropdown is the frame; HQ's charity dropdown narrows (a charity
   * caller is pinned server-side regardless). Read-only: the printed flags stay with the
   * payments vertical's own flow.
   */
  printReceiptCards(): void {
    if (this.loading || this.exporting || this.cardsPrinting) {
      return;
    }
    const batchNo = String(this.filterForm.getRawValue().batchNumber || '').trim();
    if (!batchNo) {
      this.notification.info(this.translate.instant('reports.receiptCards.pickBatch'));
      return;
    }

    const charityId = this.isHQ
      ? (String(this.filterForm.getRawValue().charityId || '').trim() || undefined)
      : undefined;

    this.cardsPrinting = true;
    this.reportService.getReceiptCards({ orpCheckBatchNo: batchNo, charityId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: report => {
          this.cardsPrinting = false;
          if (!report.rows?.length) {
            this.notification.info(
              report.message || this.translate.instant('reports.receiptCards.nothingToPrint'));
            return;
          }
          this.renderReceiptCards(report);
        },
        error: (httpError: any) => {
          this.cardsPrinting = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.receiptCards.printFailed'));
        }
      });
  }

  /**
   * §23.U.40 عرض — the batch's receipt cards, fetched at press time (cards are not grid
   * state) and previewed in the shell's shared modal: the SAME card config the print
   * command composes. Guards mirror printReceiptCards.
   */
  previewReceiptCards = (): Promise<ReportPreviewSource | null>  => {
    const batchNo = String(this.filterForm.getRawValue().batchNumber || '').trim();
    if (!batchNo) {
      this.notification.info(this.translate.instant('reports.receiptCards.pickBatch'));
      return Promise.resolve(null);
    }
    const charityId = this.isHQ
      ? (String(this.filterForm.getRawValue().charityId || '').trim() || undefined)
      : undefined;

    return firstValueFrom(this.reportService.getReceiptCards({ orpCheckBatchNo: batchNo, charityId }))
      .then(report => {
        if (!report.rows?.length) {
          this.notification.info(
            report.message || this.translate.instant('reports.receiptCards.nothingToPrint'));
          return null;
        }
        return this.reportPdfService.cardsSource(this.buildReceiptCardsConfig(report));
      });
  }

  private renderReceiptCards(report: ReceiptCardsReport): void {
    this.reportPdfService.printCardSheet(this.buildReceiptCardsConfig(report));
  }

  /** The receipt-card sheet's config — shared by the print command and the §23.U.40 preview. */
  private buildReceiptCardsConfig(report: ReceiptCardsReport): ReportCardSheetConfig<ReceiptCardRow> {
    const t = (key: string): string => this.translate.instant(key);
    const currency = report.currency ? ` ${report.currency}` : '';

    return {
      documentTitle: `${t('reports.receiptCards.title')} - ${report.batchNo}`,
      title: t('reports.receiptCards.title'),
      subtitle: `${t('reports.receiptCards.metaBatch')}: ${report.batchNo}`,
      meta: [
        { label: t('reports.receiptCards.metaCharity'), value: report.charityName || '-' },
        {
          label: t('reports.receiptCards.metaPeriod'),
          value: `${this.formatPeriod(report.periodFrom)} — ${this.formatPeriod(report.periodTo)}`
        },
        {
          label: t('reports.receiptCards.metaTotal'),
          value: `${t('reports.receiptCards.metaCount')}: ${report.totalCount} — `
            + `${t('reports.receiptCards.metaAmount')}: ${report.totalAmount}${currency}`
        }
      ],
      fields: [
        { header: t('reports.receiptCards.colCharity'), render: () => report.charityName || '-' },
        { header: t('reports.receiptCards.colOrphanCode'), render: row => row.orphanCode ?? '' },
        { header: t('reports.receiptCards.colOrphanName'), render: row => row.orphanName ?? '' },
        { header: t('reports.receiptCards.colGuardian'), render: row => row.guardianName || '' },
        {
          header: t('reports.receiptCards.colAmount'),
          render: row => `${row.amount}${currency}`
        },
        { header: t('reports.receiptCards.colCheque'), render: row => row.chiqueNo || '-' },
        { header: t('reports.receiptCards.colBatch'), render: () => report.batchNo }
      ],
      signatureLabel: t('reports.receiptCards.signature'),
      rows: report.rows
    };
  }

  /** Date helper for the meta band — short yyyy-MM-dd, dash for null. */
  private formatPeriod(value?: string | null): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return isNaN(date.getTime()) ? '-' : date.toISOString().slice(0, 10);
  }

  // ==================== search / export ====================

  /** بحث — a read; nothing stored changes. Runs from page 1 with the current filters. */
  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (!this.validateAges()) {
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

  private runSearch(): void {
    this.loading = true;
    this.reportService.getOrphanData(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => this.handleFilterError(httpError)
      });
  }

  /**
   * استخراج البيانات — the full §23.S.3 workbook over the WHOLE selection (not just the loaded
   * page), paging server-side under the 200 cap. An empty selection refuses with the
   * nothing-to-produce message instead of an empty file.
   */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (this.totalCount === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.exporting = true;
    const collected: OrphanDataRow[] = [];
    const totalPages = Math.ceil(this.totalCount / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        this.reportExportService.exportOrphanData(collected)
          .catch(() => this.notification.error(this.translate.instant('reports.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getOrphanData(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): OrphanDataFilter {
    // getRawValue keeps disabled controls (mutual exclusion disables flag checkboxes)
    const v = this.filterForm.getRawValue();
    const filter: OrphanDataFilter = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.batchNumber) {
      filter.batchNumber = String(v.batchNumber);
    }
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.governorateId && Number(v.governorateId) > 0) {
      filter.governorateId = Number(v.governorateId);
    }
    if (v.centerId && Number(v.centerId) > 0) {
      filter.centerId = Number(v.centerId);
    }
    if (v.ageFrom !== null && v.ageFrom !== undefined && v.ageFrom !== '') {
      filter.ageFrom = Number(v.ageFrom);
    }
    if (v.ageTo !== null && v.ageTo !== undefined && v.ageTo !== '') {
      filter.ageTo = Number(v.ageTo);
    }
    if (v.isFinishedSponsorship) {
      filter.isFinishedSponsorship = true;
    }
    if (v.allOrphans) {
      filter.allOrphans = true;
    }
    if (v.excluded) {
      filter.excluded = true;
    }
    // Review P27 2026-08-26: the notExcluded CHECKBOX decides — the old formula ignored it,
    // so unchecking it still sent notExcluded=true whenever the other two scopes were off.
    filter.notExcluded = !!v.notExcluded && !v.allOrphans && !v.excluded;
    return filter;
  }

  /** AC 6 mirror — refuse bad ages client-side; the server refuses again regardless. */
  private validateAges(): boolean {
    const v = this.filterForm.getRawValue();
    const ageFrom = Number(v.ageFrom);
    const ageTo = Number(v.ageTo);
    const hasFrom = v.ageFrom !== null && v.ageFrom !== undefined && v.ageFrom !== '';
    const hasTo = v.ageTo !== null && v.ageTo !== undefined && v.ageTo !== '';

    if ((hasFrom && ageFrom < 1) || (hasTo && ageTo < 1)) {
      this.notification.error(this.translate.instant('reports.orphanData.ageInvalid'));
      return false;
    }
    if (hasFrom && hasTo && ageTo < ageFrom) {
      this.notification.error(this.translate.instant('reports.orphanData.ageOrderInvalid'));
      return false;
    }
    return true;
  }

  /** Server 400s carry { message, errors } — field errors land on their controls (P-pattern). */
  private handleFilterError(httpError: any): void {
    this.loading = false;
    const errors = httpError?.details;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.filterForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);
        if (control) {
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
    }
    this.notification.error(
      httpError?.message || this.translate.instant('reports.orphanData.searchFailed')
    );
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  hasServerError(controlName: string): boolean {
    const control = this.filterForm.get(controlName);
    return !!(control && control.errors && control.errors['server'] && control.touched);
  }

  serverError(controlName: string): string {
    const control = this.filterForm.get(controlName);
    return control?.errors?.['server'] ?? '';
  }

  trackByRow(_index: number, row: OrphanDataRow): string {
    return `${row.charityId ?? ''}-${row.code}`;
  }
}
