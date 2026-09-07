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
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MissedPaymentRow } from '../models/report.model';

/**
 * UC-RPT-22 (§23.S.11 أيتام مستحقون دفعات سابقة) — the arrears grid: one row per scoped
 * orphan with at least one entitled-but-unreceived batch. The batch columns are dynamic —
 * only batch numbers present in the result set get a column (recorded decision). سبب طلب
 * الاستعداد renders a dash: no persisted arrears reason exists (real-data-only ruling).
 * Charity callers are pinned server-side; الجمعية is an HQ-only narrow.
 */
@Component({
  selector: 'app-missed-payments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './missed-payments.component.html',
  styleUrls: ['./missed-payments.component.scss']
})
export class MissedPaymentsComponent implements OnInit, OnDestroy {
  rows: MissedPaymentRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** Dynamic batch columns — the union of batch keys across the rendered rows, newest first. */
  batchColumns: string[] = [];

  /**
   * Review P27 2026-08-26: batch keys accumulate across the pages of ONE run — a page that
   * happens to lack a batch must not drop its column mid-session (rows still carrying a
   * value there would silently lose their cell). Cleared whenever a fresh run starts.
   */
  private seenBatchKeys = new Set<string>();

  charityOptions: Array<{ id: string; name: string }> = [];
  isHeadOffice = false;

  /**
   * UC-RPT-23's all-orphans toggle (جميع الدفعات الفائتة) — HQ-only widened scope. The client
   * toggle is convenience; the endpoint refuses non-HQ callers sending the flag.
   */
  allOrphans = false;

  /** The export pages through the whole grid — this endpoint's validator caps PageSize at 100. */
  private readonly exportPageSize = 100;

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: ['']
    });
  }

  ngOnInit(): void {
    // الجمعية is an HQ-only narrow — hidden for charity callers (AC 3).
    this.isHeadOffice = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHeadOffice) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — real charities endpoint only; كافة الجهات all-option. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.missedPayments.allCharities') },
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

  // ==================== commands (§23.S.11) ====================

  /** بحث — runs the arrears query. A read; nothing stored changes. */
  runSearch(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.loading = true;
    this.reportService.getMissedPayments(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.currentPage = result.page || this.currentPage;
          this.loading = false;
          this.hasRun = true;
          this.rebuildBatchColumns();
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.missedPayments.searchFailed')
          );
        }
      });
  }

  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.currentPage = 1;
    this.seenBatchKeys.clear();
    this.runSearch();
  }

  onPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  /** §23.S.11 icon command (UC-RPT-23) — toggles the widened scope and re-runs (HQ only). */
  toggleAllOrphans(): void {
    if (!this.isHeadOffice || this.loading || this.exporting) {
      return;
    }
    this.allOrphans = !this.allOrphans;
    this.currentPage = 1;
    this.seenBatchKeys.clear();
    this.runSearch();
  }

  /**
   * Review P27 2026-08-26: descending but NUMERIC-aware — plain localeCompare puts '9' above
   * '10' (and '2024-9' above '2024-12') because it compares characters, not numbers, so the
   * "newest batch left" order inverted as soon as batch numbers hit double digits.
   */
  private sortBatchKeys(keys: Iterable<string>): string[] {
    return Array.from(keys)
      .sort((a, b) => b.localeCompare(a, undefined, { numeric: true, sensitivity: 'base' }));
  }

  /** The dynamic column set: union of the batch keys seen in this run, descending (newest left). */
  private rebuildBatchColumns(): void {
    for (const row of this.rows) {
      for (const key of Object.keys(row.batchStates || {})) {
        this.seenBatchKeys.add(key);
      }
    }
    this.batchColumns = this.sortBatchKeys(this.seenBatchKeys);
  }

  /** استخراج البيانات — the arrears workbook over the WHOLE grid, paging under the 100 cap. */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (this.totalCount === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.exporting = true;
    const collected: MissedPaymentRow[] = [];
    const totalPages = Math.ceil(this.totalCount / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        // The export's column set comes from the collected rows — same dynamic rule as the grid.
        const union = new Set<string>();
        for (const row of collected) {
          for (const key of Object.keys(row.batchStates || {})) {
            union.add(key);
          }
        }
        const columns = this.sortBatchKeys(union);
        const v = this.filterForm.getRawValue();
        const scope = v.charityId ? String(v.charityId).slice(0, 8) : 'all';
        this.reportExportService.exportMissedPayments(
          collected,
          columns,
          `missed-payments_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.missedPayments.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getMissedPayments(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.missedPayments.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): { page: number; pageSize: number; charityId?: string; allOrphans?: boolean } {
    const v = this.filterForm.getRawValue();
    const filter: { page: number; pageSize: number; charityId?: string; allOrphans?: boolean } = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (this.allOrphans) {
      filter.allOrphans = true;
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: MissedPaymentRow): string {
    return row.orphanId;
  }

  trackByBatch(_index: number, batch: string): string {
    return batch;
  }
}
