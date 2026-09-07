import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanStatusReportFilter, OrphanStatusReportRow } from '../models/report.model';

/** The §23.S.5/§23.S.6 variants served by this one component (route data → predicate). */
export type OrphanStatusVariant = 'finished' | 'unsponsored';

/**
 * UC-RPT-04 (§23.S.5 أيتام انتهت كفالتهم) and UC-RPT-05 (§23.S.6 أيتام غير مكفولين) —
 * the shared orphan-status report grid. Both screens carry the SAME filter and the SAME
 * 5-column contract (spec shared-shape note); only the status predicate differs, so the
 * variant arrives from the route's data and picks the endpoint + i18n block — 18-5 extends
 * this component with a second route rather than duplicating the template (its Task 3).
 *
 * HQ-only endpoints (Gen. Director). The charity scope is enforced SERVER-SIDE — the
 * dropdown is a convenience for HQ narrowing, never the control.
 */
@Component({
  selector: 'app-finished-sponsorship-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './finished-sponsorship-report.component.html',
  styleUrls: ['./finished-sponsorship-report.component.scss']
})
export class FinishedSponsorshipReportComponent implements OnInit, OnDestroy {
  rows: OrphanStatusReportRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole selection — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  variant: OrphanStatusVariant = 'finished';

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
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

  /** i18n block for the active variant — reports.finishedSponsorship.* / reports.unsponsoredOrphans.* */
  get keyPrefix(): string {
    return this.variant === 'unsponsored'
      ? 'reports.unsponsoredOrphans'
      : 'reports.finishedSponsorship';
  }

  ngOnInit(): void {
    // Route data picks the predicate — one component, two §23 screens.
    const fromRoute = this.route.snapshot.data['orphanStatusVariant'] as OrphanStatusVariant | undefined;
    if (fromRoute === 'finished' || fromRoute === 'unsponsored') {
      this.variant = fromRoute;
    }

    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — HQ narrow; a charity caller is scoped server-side regardless. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant(`${this.keyPrefix}.allCharities`) },
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

  // ==================== search / export ====================

  /** بحث — a read; nothing stored changes. Runs from page 1 with the current filter. */
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

  private fetchPage(page: number, pageSize: number) {
    const filter = { ...this.buildFilter(page, pageSize) };
    const call = this.variant === 'unsponsored'
      ? this.reportService.getUnsponsoredOrphans(filter)
      : this.reportService.getFinishedSponsorshipOrphans(filter);
    return call;
  }

  private runSearch(): void {
    this.loading = true;
    this.fetchPage(this.currentPage, this.pageSize)
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
            httpError?.message || this.translate.instant(`${this.keyPrefix}.searchFailed`)
          );
        }
      });
  }

  /**
   * استخراج البيانات — the §23.S.5/§23.S.6 workbook over the WHOLE selection, paging
   * server-side under the 200 cap. An empty selection refuses with the nothing-to-produce
   * message (18-4's set is empty until the sponsorship state lands — recorded gap).
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
    const collected: OrphanStatusReportRow[] = [];
    const totalPages = Math.ceil(this.totalCount / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        const filePrefix = this.variant === 'unsponsored' ? 'unsponsored_orphans' : 'finished_sponsorship';
        this.reportExportService.exportOrphanStatusReport(collected, this.keyPrefix, filePrefix)
          .catch(() => this.notification.error(this.translate.instant('reports.exportFailed')));
        return;
      }

      this.fetchPage(page, this.exportPageSize)
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

  private buildFilter(page: number, pageSize: number): OrphanStatusReportFilter {
    const v = this.filterForm.getRawValue();
    const filter: OrphanStatusReportFilter = { page, pageSize };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: OrphanStatusReportRow): string {
    return `${row.charityId ?? ''}-${row.code}`;
  }
}
