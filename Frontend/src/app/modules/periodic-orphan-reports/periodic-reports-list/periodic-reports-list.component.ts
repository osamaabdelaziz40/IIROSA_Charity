import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import {
  PeriodicOrphanReportListDto,
  PeriodicOrphanReportFilterDto,
  PeriodicOrphanReportStatistics
} from '../models/periodic-orphan-report.model';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';

/**
 * Periodic reports register — §14.S.1 / UC-ORR-01.
 *
 * Spec contract: 5 filters (الجمعية · من/الي تاريخ · كود اليتيم · اسم اليتيم · أكواد)
 * plus بحث; a 6-column grid (الرقم · رقم التقرير · التاريخ · تم الاعتماد · تاريخ
 * الاعتماد · الاجراءات). Tabs are NOT in the spec — status filtering is 9-9's screen.
 * الجمعية is an HQ-only filter (a charity caller is server-pinned to its own rows).
 */
@Component({
  selector: 'app-periodic-reports-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    TranslateModule,
    PageHeaderComponent,
    PaginationComponent,
    LoadingComponent,
    EmptyStateComponent,
    DropDownComponent,
    SharedModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './periodic-reports-list.component.html',
  styleUrls: ['./periodic-reports-list.component.scss']
})
export class PeriodicReportsListComponent implements OnInit, OnDestroy {
  reports: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;
  totalPages = 0;
  loading = false;

  // Register statistics band (§14.S.1) — caller-scoped server-side like the register
  // itself; describes the caller's whole register, not the active filters.
  statistics: PeriodicOrphanReportStatistics | null = null;

  /** §14.S.1 filter bar — reactive form so the shared select2 drop-downs can bind (charity-list pattern). */
  filterForm: FormGroup;

  /** Select2 option arrays ({id, name}) fed to app-drop-down. */
  charityOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

  /** HQ sees the الجمعية drop-down (كافة الجهات = no charity pin) */
  isHeadOffice = false;
  charities: CharityDto[] = [];

  filter: PeriodicOrphanReportFilterDto = {
    pageNumber: 1,
    pageSize: 20,
    sortBy: 'CreatedOn',
    sortDirection: 'DESC'
  };

  /** Review P27 2026-08-26 precedent: teardown for the screen's subscriptions. */
  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private periodicReportService: PeriodicOrphanReportService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      charityId: ['all'],
      dateFrom: [''],
      dateTo: [''],
      orphanCode: [''],
      orphanName: [''],
      codesOnly: [false],
      reviewStatusFilter: ['all']
    });
  }

  ngOnInit(): void {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    this.buildCharityOptions();
    this.initializeStatusOptions();
    if (this.isHeadOffice) {
      // Review P30a 2026-08-24: OnPush — the async dropdown fill happens outside
      // Angular's zone-visible bindings; without markForCheck the list stays empty.
      this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            this.charities = result.items ?? [];
            this.buildCharityOptions();
            this.cdr.markForCheck();
          },
          error: () => {
            this.charities = [];
            this.buildCharityOptions();
            this.cdr.markForCheck();
          }
        });
    }

    // أكواد only toggles the grid columns (no reload) — under OnPush the form value
    // change alone doesn't re-run the *ngIfs, so drive markForCheck from it.
    this.filterForm.get('codesOnly')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.cdr.markForCheck());

    // Option labels are pre-translated (app-drop-down renders raw text) — the
    // translate pipe can't refresh them, so rebuild on a language switch.
    this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.buildCharityOptions();
        this.initializeStatusOptions();
        this.cdr.markForCheck();
      });

    this.loadReports();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** كافة الجهات sentinel — 'all' maps to no charity pin in the request. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: 'all', name: this.translate.instant('periodicReports.filters.allCharities') },
      ...this.charities.map(charity => ({ id: charity.id, name: charity.name }))
    ];
  }

  /** حالة الاعتماد filter (9-7 Task 3) — the reviewer's pending-queue entry. String ids stay verbatim. */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'all', name: this.translate.instant('periodicReports.filters.all') },
      { id: 'pending', name: this.translate.instant('periodicReports.status.pending') },
      { id: 'approved', name: this.translate.instant('periodicReports.status.approved') },
      { id: 'rejected', name: this.translate.instant('periodicReports.status.rejected') }
    ];
  }

  /** أكواد — form-backed column toggle read by the grid *ngIfs. */
  get codesOnly(): boolean {
    return !!this.filterForm.get('codesOnly')?.value;
  }

  /** بحث — applies the filter bar and returns to page 1 (§14.S.1) */
  onSearch(): void {
    this.filter.pageNumber = 1;
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;

    const filters = this.filterForm.value;
    this.filter.charityId = this.isHeadOffice && filters.charityId && filters.charityId !== 'all'
      ? filters.charityId
      : undefined;
    this.filter.reportDateFrom = filters.dateFrom || undefined;
    this.filter.reportDateTo = filters.dateTo || undefined;
    this.filter.orphanCode = (filters.orphanCode || '').trim() || undefined;
    this.filter.orphanName = (filters.orphanName || '').trim() || undefined;
    this.filter.reviewStatus = filters.reviewStatusFilter !== 'all' ? filters.reviewStatusFilter : undefined;

    this.periodicReportService.getReports(this.filter).subscribe({
      next: result => {
        this.reports = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.totalPages = result.totalPages ?? 0;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error loading periodic reports:', error);
        this.reports = [];
        this.totalCount = 0;
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * Register statistics band — describes the caller's whole register (not the active
   * filters). Silent-fail: the band is decorative context and must not surface toasts.
   * OnPush — both callbacks mark for check or the band never renders.
   */
  loadStatistics(): void {
    this.periodicReportService.getStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: statistics => {
          this.statistics = statistics;
          this.cdr.markForCheck();
        },
        error: error => {
          console.error('Error loading periodic report statistics:', error);
          this.cdr.markForCheck();
        }
      });
  }

  /** مسح التصفية — back to the §14.S.1 defaults (sentinels included) and page 1. */
  clearFilters(): void {
    this.filterForm.reset({
      charityId: 'all',
      dateFrom: '',
      dateTo: '',
      orphanCode: '',
      orphanName: '',
      codesOnly: false,
      reviewStatusFilter: 'all'
    });
    this.filter.pageNumber = 1;
    this.loadReports();
  }

  /** Whether any filter deviates from its no-filter sentinel (drives the Clear button). */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      (this.isHeadOffice && filters.charityId && filters.charityId !== 'all') ||
      filters.dateFrom ||
      filters.dateTo ||
      (filters.orphanCode || '').trim() ||
      (filters.orphanName || '').trim() ||
      filters.codesOnly ||
      filters.reviewStatusFilter !== 'all'
    );
  }

  /**
   * UC-ORR-06 — delete a register row. Confirmed first (nothing sent on decline);
   * after a successful delete the current page refreshes, stepping back one page
   * when it just emptied (15-1 stale-empty-page finding).
   */
  async deleteReport(report: PeriodicOrphanReportListDto): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('periodicReports.deleteConfirm'),
      this.translate.instant('periodicReports.deleteTitle')
    );
    if (!confirmed) {
      return;
    }

    this.periodicReportService.deleteReport(report.id).subscribe({
      next: () => {
        this.notification.show(
          this.translate.instant('periodicReports.deletedSuccessfully'),
          'success'
        );
        const pageSize = this.filter.pageSize ?? 20;
        const remainingOnPage = this.reports.length - 1;
        if (remainingOnPage === 0 && (this.filter.pageNumber ?? 1) > 1) {
          this.filter.pageNumber = (this.filter.pageNumber ?? 1) - 1;
        }
        this.loadReports();
      },
      error: error => {
        // Review P36a 2026-08-24: handleError already unwraps to the ApiResponse
        // body (or the message string) — the old error?.error?.message
        // double-unwrap always missed it, so the server's refusal reason
        // (e.g. a locked row) never surfaced.
        const reason = (typeof error === 'string' ? error : error?.message)
          || this.translate.instant('periodicReports.deleteFailed');
        this.notification.show(reason, 'error');
      }
    });
  }

  /** Only a locked row resists deletion — HQ may remove reviewed records (§14.D-25.5 A2). */
  canDelete(report: PeriodicOrphanReportListDto): boolean {
    return !report.locked;
  }

  onPageChange(page: number): void {
    this.filter.pageNumber = page;
    this.loadReports();
  }

  /** الرقم — serial continuous across pages: (page-1)*size + index + 1 */
  serial(index: number): number {
    return ((this.filter.pageNumber ?? 1) - 1) * (this.filter.pageSize ?? 20) + index + 1;
  }

  trackByReportId(_index: number, report: PeriodicOrphanReportListDto): string {
    return report.id;
  }

  /** تم الاعتماد badge class from the computed reviewStatus.
   *  Review P49 2026-08-24: Bootstrap 5.3 colour utilities are bg-*, not the
   *  Bootstrap 4 badge-* names — the old classes styled nothing on this stack. */
  statusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'bg-success';
      case 'rejected': return 'bg-danger';
      default: return 'bg-warning text-dark';
    }
  }

  /** Edit is offered while the row is not locked and not accepted — refused rows reopen for resubmission (§14.D-25.5 A1) */
  canEdit(report: PeriodicOrphanReportListDto): boolean {
    return !report.locked && !report.isAccepted;
  }

  hasPermission(permission: string): boolean {
    return this.auth.hasPermission(permission);
  }
}
