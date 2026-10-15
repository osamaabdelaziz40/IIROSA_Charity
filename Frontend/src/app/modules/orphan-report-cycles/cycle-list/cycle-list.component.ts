import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription, debounceTime, distinctUntilChanged } from 'rxjs';

import { OrphanReportCycleService } from '../services/orphan-report-cycle.service';
import {
  OrphanReportCycleDto,
  OrphanReportCycleStatus
} from '../models/orphan-report-cycle.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem, PageHeaderComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

/**
 * UC-ORC-01 — the HQ cycles register (§26.S.1). Status is server-derived from the
 * dates (BR-ORC-02) and rendered read-only; the only writes are master data (UC-ORC-03,
 * refused once completed) and delete (UC-ORC-04, only a never-started cycle with zero
 * bound reports — the button renders just for those, the endpoint stays the authority).
 */
@Component({
  selector: 'app-cycle-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './cycle-list.component.html',
  styleUrls: ['./cycle-list.component.scss']
})
export class CycleListComponent implements OnInit, OnDestroy {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'orphanReportCycles.title' }
  ];

  pageActions = [
    {
      label: 'orphanReportCycles.addCycle',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createCycle()
    }
  ];

  cycles: OrphanReportCycleDto[] = [];
  loading = false;
  totalCount = 0;
  deletingId?: string;

  filterForm: FormGroup;
  currentPage = 1;
  pageSize = 10;

  /**
   * Select2 option labels are pre-translated (app-drop-down renders raw text) —
   * rebuilt on language switch only, and kept as a stable reference so the widget
   * is not re-initialised on every change-detection pass.
   */
  statusOptions: Array<{ id: string; name: string }> = [];
  private langChangeSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private cycleService: OrphanReportCycleService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      status: ['all']
    });
  }

  ngOnInit(): void {
    this.initializeStatusOptions();
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeStatusOptions();
    });

    // Debounced free-text search — one request per burst of keystrokes.
    this.filterForm.controls['searchValue'].valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.onFilterChange());

    this.loadCycles();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  /** Status options carry string ids — 'all' is the no-filter sentinel. */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: String(OrphanReportCycleStatus.NotStarted), name: this.translate.instant('orphanReportCycles.status.notStarted') },
      { id: String(OrphanReportCycleStatus.InProgress), name: this.translate.instant('orphanReportCycles.status.inProgress') },
      { id: String(OrphanReportCycleStatus.Completed), name: this.translate.instant('orphanReportCycles.status.completed') }
    ];
  }

  loadCycles(): void {
    this.loading = true;

    const values = this.filterForm.value;
    this.cycleService.getCycles({
      search: values.searchValue || undefined,
      status: values.status !== 'all' && values.status !== '' ? Number(values.status) : undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: result => {
        this.cycles = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.cycles = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadCycles();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCycles();
  }

  clearFilters(): void {
    // emitEvent: false — the reload below covers it; the reset must not re-fire the
    // debounced searchValue subscription too.
    this.filterForm.reset({ searchValue: '', status: 'all' }, { emitEvent: false });
    this.onFilterChange();
  }

  hasActiveFilters(): boolean {
    const values = this.filterForm.value;
    return !!(values.searchValue || (values.status && values.status !== 'all'));
  }

  createCycle(): void {
    this.router.navigate(['/orphan-report-cycles', 'create']);
  }

  editCycle(cycle: OrphanReportCycleDto): void {
    this.router.navigate(['/orphan-report-cycles', cycle.id, 'edit']);
  }

  /** UC-ORC-04 — offered only for a never-started cycle; the endpoint re-checks reports. */
  async deleteCycle(cycle: OrphanReportCycleDto): Promise<void> {
    if (this.deletingId || cycle.status !== OrphanReportCycleStatus.NotStarted) {
      return;
    }

    const confirmed = await this.notification.confirm(
      this.translate.instant('orphanReportCycles.list.deleteConfirm', { title: cycle.titleAr })
    );
    if (!confirmed) {
      return;
    }

    this.deletingId = cycle.id;
    this.cycleService.deleteCycle(cycle.id).subscribe({
      next: () => {
        this.deletingId = undefined;
        this.notification.success(this.translate.instant('orphanReportCycles.list.deleted'));
        this.loadCycles();
      },
      error: (error: any) => {
        this.deletingId = undefined;
        this.notification.error(
          error?.error?.message || this.translate.instant('orphanReportCycles.list.deleteFailed'));
      }
    });
  }

  isCompleted(cycle: OrphanReportCycleDto): boolean {
    return cycle.status === OrphanReportCycleStatus.Completed;
  }

  /** Template guard for the delete action (UC-ORC-04) — the enum is unreachable in markup. */
  isNotStarted(cycle: OrphanReportCycleDto): boolean {
    return cycle.status === OrphanReportCycleStatus.NotStarted;
  }

  statusKey(cycle: OrphanReportCycleDto): string {
    switch (cycle.status) {
      case OrphanReportCycleStatus.InProgress: return 'orphanReportCycles.status.inProgress';
      case OrphanReportCycleStatus.Completed: return 'orphanReportCycles.status.completed';
      default: return 'orphanReportCycles.status.notStarted';
    }
  }

  statusClass(cycle: OrphanReportCycleDto): string {
    switch (cycle.status) {
      case OrphanReportCycleStatus.InProgress: return 'badge-success';
      case OrphanReportCycleStatus.Completed: return 'badge-secondary';
      default: return 'badge-warning';
    }
  }

  getSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByCycleId = (_: number, cycle: OrphanReportCycleDto): string => cycle.id;
}
