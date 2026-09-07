import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  OrphansMissingReportsSummaryRow,
  OrphansMissingReportsDetailRow
} from '../models/report.model';

/**
 * UC-RPT-15 (§23.S.14 أيتام مكودون مطلوب لهم تقرير) — the pre-payment-run chase summary.
 *
 * The summary grid counts, per charity, the coded orphans with no accepted report covering
 * the trailing 12 months (BR-11 semantics — the same chase set as the §14.U.14 screen).
 * ExtractDetails drills one charity's list; a charity caller is server-clamped to its own
 * rows whatever the payload names (pin-never-widen).
 */
@Component({
  selector: 'app-orphans-missing-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    PaginationComponent,
    DropDownComponent
  ],
  templateUrl: './orphans-missing-reports.component.html',
  styleUrls: ['./orphans-missing-reports.component.scss']
})
export class OrphansMissingReportsComponent implements OnInit, OnDestroy {
  // §23.S.14 summary — one row per charity
  rows: OrphansMissingReportsSummaryRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole selection — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  // ExtractDetails drill-down — one charity's chase list
  detailRows: OrphansMissingReportsDetailRow[] = [];
  detailLoading = false;
  detailCurrentPage = 1;
  detailPageSize = 20;
  detailTotalCount = 0;
  selectedCharityId = '';
  selectedCharityName = '';

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

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
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
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
            { id: '', name: this.translate.instant('reports.orphansMissingReports.allCharities') },
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

  // ==================== summary (بحث) ====================

  /** بحث/load — a read; nothing stored changes. Re-runs from page 1 and closes the drill. */
  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.currentPage = 1;
    this.closeDetail();
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
    this.reportService.getOrphansMissingReports(this.buildFilter())
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
            httpError?.message || this.translate.instant('reports.orphansMissingReports.searchFailed')
          );
        }
      });
  }

  // ==================== ExtractDetails drill-down ====================

  /** ExtractDetails(row.Id) — loads that charity's chase list; the server clamps scope. */
  drillDown(row: OrphansMissingReportsSummaryRow): void {
    if (!row.charityId || this.detailLoading) {
      return;
    }
    this.selectedCharityId = row.charityId;
    this.selectedCharityName = row.charityName || '';
    this.detailCurrentPage = 1;
    this.runDetailSearch();
  }

  onDetailPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.detailCurrentPage = page;
    this.runDetailSearch();
  }

  closeDetail(): void {
    this.selectedCharityId = '';
    this.selectedCharityName = '';
    this.detailRows = [];
    this.detailTotalCount = 0;
    this.detailCurrentPage = 1;
  }

  private runDetailSearch(): void {
    this.detailLoading = true;
    this.reportService
      .getOrphansMissingReportsDetail(this.selectedCharityId, this.detailCurrentPage, this.detailPageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.detailRows = result.items || [];
          this.detailTotalCount = result.totalCount || 0;
          this.detailLoading = false;
        },
        error: (httpError: any) => {
          this.detailLoading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.orphansMissingReports.detailFailed')
          );
        }
      });
  }

  // ==================== export ====================

  /**
   * استخراج البيانات — exports the ACTIVE grid over the whole selection, paging server-side
   * under the cap: the drill-down list when one is open, otherwise the summary. An empty
   * selection refuses with the nothing-to-produce message.
   */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (this.totalCount === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    if (this.selectedCharityId) {
      this.exportDetail();
      return;
    }
    this.exportSummary();
  }

  private exportSummary(): void {
    this.exporting = true;
    const collected: OrphansMissingReportsSummaryRow[] = [];
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
        this.reportExportService.exportOrphansMissingReportsSummary(
          collected,
          `orphans-missing-reports_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.orphansMissingReports.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getOrphansMissingReports(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.orphansMissingReports.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  private exportDetail(): void {
    this.exporting = true;
    const collected: OrphansMissingReportsDetailRow[] = [];
    const totalPages = Math.ceil(this.detailTotalCount / this.exportPageSize);
    const scope = this.selectedCharityId.slice(0, 8);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        this.reportExportService.exportOrphansMissingReportsDetails(collected, scope)
          .catch(() => this.notification.error(this.translate.instant('reports.orphansMissingReports.exportFailed')));
        return;
      }

      this.reportService
        .getOrphansMissingReportsDetail(this.selectedCharityId, page, this.exportPageSize)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.orphansMissingReports.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): { page: number; pageSize: number; charityId?: string } {
    const v = this.filterForm.getRawValue();
    const filter: { page: number; pageSize: number; charityId?: string } = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  detailSerial(index: number): number {
    return (this.detailCurrentPage - 1) * this.detailPageSize + index + 1;
  }

  trackByRow(_index: number, row: OrphansMissingReportsSummaryRow): string {
    return row.charityId ?? 'no-charity';
  }

  trackByDetailRow(_index: number, row: OrphansMissingReportsDetailRow): string {
    return row.orphanId;
  }
}
