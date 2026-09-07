import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { NonRenewedOrphanRow } from '../models/report.model';

/**
 * UC-RPT-19 (§23.U.19 أيتام بدون تقرير مجدد) — the batch chase list: for the caller's
 * (or the selected) charity and a payment batch, the batch's orphans whose report was not
 * renewed for the current cycle. An absent batch means the current one — the latest
 * payment group by GroupDate. Read-only; the scoping is the endpoint's, never the menu's.
 *
 * Distinct from the §14.U.14 window chase list (`orphan-reports/non-renewed`) — same
 * endpoint, batch mode.
 */
@Component({
  selector: 'app-batch-non-renewed-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './batch-non-renewed-reports.component.html',
  styleUrls: ['./batch-non-renewed-reports.component.scss']
})
export class BatchNonRenewedReportsComponent implements OnInit, OnDestroy {
  rows: NonRenewedOrphanRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole list — this endpoint's validator caps PageSize at 100. */
  private readonly exportPageSize = 100;

  isHQ = false;

  charityOptions: Array<{ id: string; name: string }> = [];
  batchOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private orphanPaymentService: OrphanPaymentService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      batchId: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
    // The batch picker follows the HQ charity narrow (10-6 pattern); a charity caller's
    // list is token-scoped server-side already.
    this.filterForm.get('charityId')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadBatches());
    this.loadBatches();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — real charities endpoint only; كل الجهات all-option for HQ. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.nonRenewed.allCharities') },
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

  /** رقم الدفعة — distinct batch numbers in the caller's scope (18-1's reuse rule). */
  private loadBatches(): void {
    const charityNarrow = this.isHQ
      ? (this.filterForm.get('charityId')!.value || undefined)
      : undefined;
    this.orphanPaymentService.getBatchNumbers(charityNarrow)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.batchOptions = [
            { id: '', name: this.translate.instant('reports.nonRenewed.currentBatch') },
            ...(options || []).map(b => ({ id: b.batchNo, name: b.batchNo }))
          ];
        },
        error: () => console.error('Error loading batch numbers')
      });
  }

  // ==================== search / export ====================

  /** بحث/load — a read; nothing stored changes. Runs from page 1 with the current filter. */
  search(): void {
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

  private runSearch(): void {
    this.loading = true;
    this.reportService.getNonRenewedReports(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount ?? result.count ?? 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.nonRenewed.searchFailed')
          );
        }
      });
  }

  /**
   * استخراج البيانات — the §23.U.19 workbook over the WHOLE list, paging server-side
   * under the 100 cap. An empty list refuses with the nothing-to-produce message.
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
    const collected: NonRenewedOrphanRow[] = [];
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
        this.reportExportService.exportNonRenewedReports(
          collected,
          `non-renewed-reports_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.nonRenewed.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getNonRenewedReports(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.nonRenewed.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): { page: number; pageSize: number; charityId?: string; batchId?: string } {
    const v = this.filterForm.getRawValue();
    const filter: { page: number; pageSize: number; charityId?: string; batchId?: string } = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.batchId) {
      filter.batchId = String(v.batchId);
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: NonRenewedOrphanRow): string {
    return row.orphanId;
  }
}
