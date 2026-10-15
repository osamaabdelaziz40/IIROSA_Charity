import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { OrphanReportCycleService } from '../../orphan-report-cycles/services/orphan-report-cycle.service';
import {
  OrphanReportCycleSelectorDto,
  OrphanReportCycleStatus,
  CycleReportCoverageRowDto
} from '../../orphan-report-cycles/models/orphan-report-cycle.model';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem, PaginationComponent } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';

/**
 * UC-ORC-09 cycle coverage (§26.S.4) — the coded-orphan population of the caller's
 * scope split into reported / not-reported for one chosen cycle. HQ callers may narrow
 * by charity; a charity caller is pinned server-side and sees no charity filter.
 *
 * Tabs page one Category at a time (Submitted / NotSubmitted / All interleaved); the
 * two headline counts come with every response, so the tab badges stay honest even on
 * the All view.
 */
@Component({
  selector: 'app-cycle-report-coverage',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    BreadcrumbComponent,
    PaginationComponent,
    DropDownComponent,
    SharedModule
  ],
  templateUrl: './cycle-report-coverage.component.html',
  styleUrls: ['./cycle-report-coverage.component.scss']
})
export class CycleReportCoverageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'reports.title', url: '/reports/all' },
    { label: 'reports.cycleCoverage.title' }
  ];

  filterForm: FormGroup;

  /** Every cycle (all statuses — completed rounds stay reviewable). */
  cycleOptions: Array<{ id: string; name: string }> = [];
  /** HQ narrowing; hidden for charity-scoped callers. */
  isSuperAdmin = false;
  charityOptions: Array<{ id: string | null; name: string }> = [];

  rows: CycleReportCoverageRowDto[] = [];
  totalCount = 0;
  submittedCount = 0;
  notSubmittedCount = 0;
  cycleTitle = '';
  loading = false;
  /** The tab currently paged: '' = All, 'Submitted', 'NotSubmitted'. */
  category = '';

  currentPage = 1;
  pageSize = 20;

  constructor(
    private fb: FormBuilder,
    private cycleService: OrphanReportCycleService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      cycleId: [''],
      charityId: [null],
      searchValue: ['']
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.loadCycleOptions();
    if (this.isSuperAdmin) {
      this.loadCharityOptions();
    }

    // Debounced free-text reload; the cycle/charity dropdowns reload on change events.
    this.filterForm.get('searchValue')!.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.run();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** All cycles, newest first, the live status folded into the label. */
  private loadCycleOptions(): void {
    this.cycleService.getSelector().subscribe({
      next: cycles => {
        this.cycleOptions = cycles.map(c => ({
          id: c.id,
          name: `${this.titleOf(c)} · ${this.statusName(c)}`
        }));
        this.cdr.markForCheck();
      },
      error: () => {
        this.cycleOptions = [];
        this.cdr.markForCheck();
      }
    });
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.charityOptions = [
          { id: null, name: this.translate.instant('orphanCoding.allCharities') },
          ...(result.items || []).map(c => ({ id: c.id, name: c.name }))
        ];
        this.cdr.markForCheck();
      },
      error: () => {
        this.charityOptions = [];
        this.cdr.markForCheck();
      }
    });
  }

  private titleOf(cycle: OrphanReportCycleSelectorDto): string {
    return cycle.titleAr || cycle.titleEn || cycle.id;
  }

  private statusName(cycle: OrphanReportCycleSelectorDto): string {
    switch (cycle.status) {
      case OrphanReportCycleStatus.InProgress: return this.translate.instant('orphanReportCycles.status.inProgress');
      case OrphanReportCycleStatus.Completed: return this.translate.instant('orphanReportCycles.status.completed');
      default: return this.translate.instant('orphanReportCycles.status.notStarted');
    }
  }

  /** The cycle is mandatory — nothing runs until one is chosen. */
  run(): void {
    const cycleId = this.filterForm.get('cycleId')?.value;
    if (!cycleId) {
      this.notification.warning(this.translate.instant('reports.cycleCoverage.pickCycleFirst'));
      return;
    }

    this.loading = true;
    this.cdr.markForCheck();

    this.cycleService.getCoverage({
      cycleId,
      charityId: this.filterForm.get('charityId')?.value || undefined,
      search: (this.filterForm.get('searchValue')?.value || '').trim() || undefined,
      category: this.category || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: result => {
        this.rows = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.submittedCount = result.submittedCount || 0;
        this.notSubmittedCount = result.notSubmittedCount || 0;
        this.cycleTitle = result.cycleTitle || '';
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        this.loading = false;
        this.rows = [];
        this.totalCount = 0;
        this.notification.error(
          error?.error?.message || this.translate.instant('reports.cycleCoverage.loadFailed'));
        this.cdr.markForCheck();
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    // A different cycle resets the tab — the counts belong to the new cycle.
    this.category = '';
    this.run();
  }

  setCategory(category: string): void {
    if (this.category === category) {
      return;
    }
    this.category = category;
    this.currentPage = 1;
    this.run();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.run();
  }

  /** Review-state chip translation ('Accepted' | 'Refused' | 'Pending' from the server). */
  reviewStateKey(state?: string): string {
    switch (state) {
      case 'Accepted': return 'orphanReportCycles.review.accepted';
      case 'Refused': return 'orphanReportCycles.review.refused';
      default: return 'orphanReportCycles.review.pending';
    }
  }

  reviewStateClass(state?: string): string {
    switch (state) {
      case 'Accepted': return 'badge-success';
      case 'Refused': return 'badge-danger';
      default: return 'badge-warning';
    }
  }

  getSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByOrphanId = (_: number, row: CycleReportCoverageRowDto): string => row.orphanId;
}
