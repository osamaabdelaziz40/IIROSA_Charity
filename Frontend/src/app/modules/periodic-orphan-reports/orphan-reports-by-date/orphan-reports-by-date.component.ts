import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { FamilyService } from '../../families/services/family.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanCodingSearchRequest, OrphanLookupDto } from '../../families/models/family.model';
import {
  PeriodicOrphanReportListDto,
  PeriodicOrphanReportFilterDto
} from '../models/periodic-orphan-report.model';
import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

/**
 * Orphan-centric report register: pick an orphan by name or sponsorship code
 * (UC-ORP-02 shared read), then page through their periodic reports ordered by
 * report date (newest first), with an optional من/إلى date window.
 *
 * The read rides the ordinary §14.S.1 filter endpoint with orphanId + sortBy —
 * the caller's charity scope stays enforced server-side, exactly like the register.
 */
@Component({
  selector: 'app-orphan-reports-by-date',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    PaginationComponent,
    SharedModule
  ],
  templateUrl: './orphan-reports-by-date.component.html',
  styleUrls: ['./orphan-reports-by-date.component.scss']
})
export class OrphanReportsByDateComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // ===== Orphan picker (state A — no subject selected yet) =====
  searchForm: FormGroup;
  orphans: OrphanLookupDto[] = [];
  orphanTotalCount = 0;
  orphanCurrentPage = 1;
  orphanPageSize = 20;
  orphansLoading = false;

  // ===== Selected subject + their reports (state B) =====
  selectedOrphan?: OrphanLookupDto;
  reports: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  reportsLoading = false;
  dateForm: FormGroup;

  canEditReports = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'periodicReports.title', url: '/periodic-orphan-reports' },
    { label: 'periodicReports.orphanReportsByDate.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private familyService: FamilyService,
    private periodicReportService: PeriodicOrphanReportService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.searchForm = this.fb.group({ searchValue: [''] });
    this.dateForm = this.fb.group({ reportDateFrom: [null], reportDateTo: [null] });
  }

  ngOnInit(): void {
    this.canEditReports = this.authService.hasPermission('PeriodicReports.Edit');

    // Type-ahead: one request after the caller stops typing, not one per keystroke.
    this.searchForm.get('searchValue')!.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.orphanCurrentPage = 1;
        this.loadOrphans();
      });

    this.loadOrphans();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** The plain UC-ORP-02 search — every role, charity scope enforced server-side. */
  loadOrphans(): void {
    this.orphansLoading = true;
    this.cdr.markForCheck();

    const search = (this.searchForm.get('searchValue')?.value || '').trim();
    const request: OrphanCodingSearchRequest = {
      search: search || undefined,
      pageNumber: this.orphanCurrentPage,
      pageSize: this.orphanPageSize
    };

    this.familyService.searchOrphansCoding(request).subscribe({
      next: response => {
        this.orphans = response.items || [];
        this.orphanTotalCount = response.totalCount || 0;
        this.orphansLoading = false;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error searching orphans:', error);
        this.notification.error(error?.error?.message || error?.message
          || this.translate.instant('periodicReports.orphanReportsByDate.loadOrphansFailed'));
        this.orphansLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onOrphanPageChange(page: number): void {
    this.orphanCurrentPage = page;
    this.loadOrphans();
  }

  /** Pick the subject and load their reports, newest report date first. */
  selectOrphan(orphan: OrphanLookupDto): void {
    this.selectedOrphan = orphan;
    this.currentPage = 1;
    this.dateForm.reset({ reportDateFrom: null, reportDateTo: null });
    this.loadReports();
  }

  changeOrphan(): void {
    this.selectedOrphan = undefined;
    this.reports = [];
    this.totalCount = 0;
    this.cdr.markForCheck();
  }

  onDateFilterChange(): void {
    this.currentPage = 1;
    this.loadReports();
  }

  private loadReports(): void {
    const orphan = this.selectedOrphan;
    if (!orphan) {
      return;
    }
    this.reportsLoading = true;
    this.cdr.markForCheck();

    const filter: PeriodicOrphanReportFilterDto = {
      orphanId: orphan.orphanId,
      sortBy: 'reportdate',
      sortDirection: 'desc',
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };
    const from = this.dateForm.get('reportDateFrom')?.value;
    const to = this.dateForm.get('reportDateTo')?.value;
    if (from) {
      filter.reportDateFrom = from;
    }
    if (to) {
      filter.reportDateTo = to;
    }

    this.periodicReportService.getReports(filter).subscribe({
      next: result => {
        this.reports = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.reportsLoading = false;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error loading orphan reports:', error);
        this.notification.error(error?.error?.message || error?.message
          || this.translate.instant('periodicReports.orphanReportsByDate.loadReportsFailed'));
        this.reportsLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadReports();
  }

  viewReport(report: PeriodicOrphanReportListDto): void {
    this.router.navigate(['/periodic-orphan-reports', report.id]);
  }

  editReport(report: PeriodicOrphanReportListDto): void {
    this.router.navigate(['/periodic-orphan-reports', report.id, 'edit']);
  }

  printReport(report: PeriodicOrphanReportListDto): void {
    this.router.navigate(['/periodic-orphan-reports', report.id, 'print']);
  }

  /** Register idiom (§14.S.1) — pending rows are the amber default. */
  statusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'bg-success';
      case 'rejected': return 'bg-danger';
      default: return 'bg-warning text-dark';
    }
  }

  /** Edit is offered while the row is not locked and not accepted — refused rows reopen (§14.D-25.5 A1). */
  canEdit(report: PeriodicOrphanReportListDto): boolean {
    return !report.locked && !report.isAccepted;
  }

  getOrphanSerial(index: number): number {
    return (this.orphanCurrentPage - 1) * this.orphanPageSize + index + 1;
  }

  getReportSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByOrphanId = (_: number, orphan: OrphanLookupDto): string => orphan.orphanId;
  trackByReportId = (_: number, report: PeriodicOrphanReportListDto): string => report.id;
}
