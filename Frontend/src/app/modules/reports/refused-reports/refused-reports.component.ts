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
import { RefusedReportsRow } from '../models/report.model';

/**
 * UC-RPT-18 (§23.S.17 تقارير تم رفضها) — the refused worklist, forked from 18-17's shared
 * grid: refused reports with سبب الرفض resolved (catalogue label preferred, free-text
 * fallback). The decision write path is NOT here — the jump opens the periodic-report
 * review screen (`{id}/review`), which authorises and records; the jump shows only for
 * review-capable roles ({id}/review's own [Authorize] is the control, this is convenience).
 */
@Component({
  selector: 'app-refused-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './refused-reports.component.html',
  styleUrls: ['./refused-reports.component.scss']
})
export class RefusedReportsComponent implements OnInit, OnDestroy {
  rows: RefusedReportsRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole worklist — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  isHQ = false;

  /** Mirrors {id}/review's roles — SuperAdmin, Admin, Accountant, Employee. */
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
    // Convenience only — the endpoint's [Authorize] is the control (no client-only authorisation).
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
            { id: '', name: this.translate.instant('reports.refusedReports.allCharities') },
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
    this.reportService.getRefusedReports(this.buildFilter())
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
            httpError?.message || this.translate.instant('reports.refusedReports.searchFailed')
          );
        }
      });
  }

  /**
   * EditOrpReport — the decision lives on the review screen; it authorises and records.
   * Hidden for callers outside {id}/review's roles (convenience — the endpoint re-checks).
   */
  openReview(reportId: string): void {
    this.router.navigate(['/periodic-orphan-reports', reportId, 'review']);
  }

  /**
   * استخراج البيانات (ExportReportData) — the §23.S.17 workbook over the WHOLE worklist,
   * paging server-side under the 200 cap. An empty worklist refuses with the
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
    const collected: RefusedReportsRow[] = [];
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
        this.reportExportService.exportRefusedReports(
          collected,
          `refused-reports_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`
        ).catch(() => this.notification.error(this.translate.instant('reports.refusedReports.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getRefusedReports(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.refusedReports.exportFailed'));
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

  trackByRow(_index: number, row: RefusedReportsRow): string {
    return row.reportId;
  }
}
