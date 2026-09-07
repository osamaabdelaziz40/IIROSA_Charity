import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReportsAwaitingApprovalRow } from '../models/report.model';

/**
 * UC-RPT-17 (§23.S.16 تقارير في انتظار الموافقة) — the HQ review queue: submitted periodic
 * reports that are neither accepted nor refused, oldest first. Pattern-holder for the shared
 * §23.S.16/§23.S.17 grid — 18-18 forks this component with the refused filter. The
 * EditOrpReport jump targets /periodic-orphan-reports/{id}/review (registered by 18-14's
 * audit — the story's "degrade until 18-2 lands" premise was stale).
 */
@Component({
  selector: 'app-reports-awaiting-approval',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './reports-awaiting-approval.component.html',
  styleUrls: ['./reports-awaiting-approval.component.scss']
})
export class ReportsAwaitingApprovalComponent implements OnInit, OnDestroy {
  rows: ReportsAwaitingApprovalRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole queue — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  isHQ = false;
  /**
   * Review P27 2026-08-26: the review jump is a WRITE command — gated on the reviewer roles
   * (18-18's refused-reports pattern: canReview), not offered to read-only viewers of the queue.
   */
  canReview = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
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
    // Review P27 2026-08-26: reviewer roles for the review jump (18-18 sibling idiom).
    this.canReview = this.authService.hasAnyRole(['SuperAdmin', 'Admin', 'Accountant', 'Employee']);
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
            { id: '', name: this.translate.instant('reports.awaitingApproval.allCharities') },
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

  // ==================== search / export / jump ====================

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
    this.reportService.getReportsAwaitingApproval(this.buildFilter())
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
            httpError?.message || this.translate.instant('reports.awaitingApproval.searchFailed')
          );
        }
      });
  }

  /**
   * EditOrpReport — the review jump. The route is registered (18-14's audit landed it);
   * its own PermissionGuard decides whether this caller may review.
   */
  openReview(reportId: string): void {
    this.router.navigate(['/periodic-orphan-reports', reportId, 'review']);
  }

  /**
   * استخراج البيانات (ExportReportData) — the §23.S.16 workbook over the WHOLE queue,
   * paging server-side under the 200 cap. An empty queue refuses with the
   * nothing-to-produce message.
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
    const collected: ReportsAwaitingApprovalRow[] = [];
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
        this.reportExportService.exportReportsAwaitingApproval(
          collected,
          `reports-awaiting-approval_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.awaitingApproval.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getReportsAwaitingApproval(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.awaitingApproval.exportFailed'));
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

  trackByRow(_index: number, row: ReportsAwaitingApprovalRow): string {
    return row.reportId;
  }
}
