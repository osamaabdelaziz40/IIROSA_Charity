/**
 * Mission List Component (epic 15, UC-MSN-01/02)
 * The §20.S.1 register: charity + date filters, 13-column grid, serial numbering.
 * Loads via my-missions (caller-scoped register read); بحث re-queries GET /api/MissionManagement.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import { Mission, MissionSearchRequest } from '../models/mission.model';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent, BreadcrumbComponent, PageHeaderComponent, BreadcrumbItem, DropDownComponent } from '../../../shared/components';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-mission-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PaginationComponent, BreadcrumbComponent, PageHeaderComponent, DropDownComponent],
  templateUrl: './mission-list.component.html',
  styleUrls: ['./mission-list.component.scss']
})
export class MissionListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  missions: Mission[] = [];
  charities: CharityDto[] = [];
  users: Array<{ id: string; fullName?: string; userName?: string; email?: string }> = [];

  // Loading states
  loading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // §20.S.1 filters: الجمعية + من تاريخ / الي تاريخ — bound to filterForm
  // (charities-list pattern; the shared select2 drop-down needs a FormGroup).
  filterForm: FormGroup;

  // Select2 option array ({id, name}) fed to app-drop-down.
  charityOptions: Array<{ id: string; name: string }> = [];
  userOptions: Array<{ id: string; name: string }> = [];

  /** Sentinel id meaning "all" for the charity/user filter drop-downs — maps to no filter. */
  private static readonly ALL = 'all';

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'missions.title' }
  ];

  // Page actions
  pageActions: PageAction[] = [
    {
      label: 'missions.addMission',
      icon: 'fe-plus',
      type: 'primary',
      click: () => this.createMission()
    },
    {
      label: 'common.exportToExcel',
      icon: 'fe-download',
      type: 'secondary',
      click: () => this.exportToExcel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    protected missionService: MissionService,
    protected charityService: CharityService,
    protected userManagementService: UserManagementService,
    protected auth: AuthService,
    protected router: Router,
    protected translate: TranslateService,
    protected notification: NotificationService
  ) {
    this.filterForm = this.fb.group({
      charityId: [MissionListComponent.ALL],
      assignedToUserId: [MissionListComponent.ALL],
      search: [''],
      dateFrom: [''],
      dateTo: ['']
    });
  }

  ngOnInit(): void {
    this.buildCharityOptions();
    this.buildUserOptions();
    this.loadCharities();
    this.loadUsers();
    this.loadMissions();

    // The "كافة الجهات"/"كافة المستخدمين" option labels are pre-translated
    // (app-drop-down renders raw text), so a language switch needs a rebuild —
    // the translate pipe can't refresh them.
    this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.buildCharityOptions();
        this.buildUserOptions();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * UC-MSN-08: deletion is the General Director's alone. The endpoint enforces it; this
   * only keeps the button from offering an action the server would refuse.
   */
  get canDelete(): boolean {
    return this.auth.hasPermission('Missions.Delete');
  }

  /**
   * Load charities for the filter drop-down
   */
  loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.charities = result.items || [];
          this.buildCharityOptions();
        },
        error: () => {
          // The filter degrades to كافة الجهات only — not fatal
          this.charities = [];
          this.buildCharityOptions();
        }
      });
  }

  /** Filter options — an explicit "كافة الجهات" sentinel first, then one entry per charity. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: MissionListComponent.ALL, name: this.translate.instant('missions.allCharities') },
      ...this.charities.map(c => ({ id: c.id, name: c.name }))
    ];
  }

  /**
   * Load users for the assigned-to filter drop-down — the same identity store the
   * create form's picker uses (/api/usermanagement), so every offered value matches
   * an FK_UserId the register can actually hold.
   */
  loadUsers(): void {
    this.userManagementService.getUsers({ page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.users = result.items || [];
          this.buildUserOptions();
        },
        error: () => {
          // The filter degrades to كافة المستخدمين only — not fatal
          this.users = [];
          this.buildUserOptions();
        }
      });
  }

  /** Assigned-user filter options — a "كافة المستخدمين" sentinel first, then every user. */
  private buildUserOptions(): void {
    this.userOptions = [
      { id: MissionListComponent.ALL, name: this.translate.instant('missions.allUsers') },
      ...this.users.map(u => ({ id: u.id, name: u.fullName || u.userName || u.email }))
    ];
  }

  /**
   * Load the register (initial load + pagination) — the caller-scoped my-missions read
   */
  loadMissions(): void {
    this.loading = true;

    const filters = this.filterForm.value;
    const request: MissionSearchRequest = {
      charityId: filters.charityId && filters.charityId !== MissionListComponent.ALL ? filters.charityId : undefined,
      assignedToUserId: filters.assignedToUserId && filters.assignedToUserId !== MissionListComponent.ALL ? filters.assignedToUserId : undefined,
      search: filters.search?.trim() || undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.missionService.getMyMissions(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.missions = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('missions.loadFailed'));
        }
      });
  }

  /**
   * Serial column — continuous across pages (13-1 formula)
   */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  /**
   * بحث (UC-MSN-02): date-range search against GET /api/MissionManagement. Inverted
   * ranges are refused client-side with a translated message; the server tolerates them.
   */
  onSearch(): void {
    const filters = this.filterForm.value;
    if (filters.dateFrom && filters.dateTo && filters.dateFrom > filters.dateTo) {
      this.notification.error(this.translate.instant('missions.invalidDateRange'));
      return;
    }

    this.currentPage = 1;
    this.loading = true;

    const request: MissionSearchRequest = {
      charityId: filters.charityId && filters.charityId !== MissionListComponent.ALL ? filters.charityId : undefined,
      assignedToUserId: filters.assignedToUserId && filters.assignedToUserId !== MissionListComponent.ALL ? filters.assignedToUserId : undefined,
      search: filters.search?.trim() || undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.missionService.getMissions(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.missions = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('missions.loadFailed'));
        }
      });
  }

  /**
   * Clear both dates (and the charity filter) and return to the unfiltered register
   */
  clearFilters(): void {
    this.filterForm.reset({
      charityId: MissionListComponent.ALL,
      assignedToUserId: MissionListComponent.ALL,
      search: '',
      dateFrom: '',
      dateTo: ''
    });
    this.currentPage = 1;
    this.loadMissions();
  }

  /** Any filter off its default? Drives the conditional Clear Filters button. */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      (filters.charityId && filters.charityId !== MissionListComponent.ALL) ||
      (filters.assignedToUserId && filters.assignedToUserId !== MissionListComponent.ALL) ||
      filters.search?.trim() ||
      filters.dateFrom ||
      filters.dateTo
    );
  }

  // ========== Actions (الاجراءات) ==========

  viewMission(id: string): void {
    this.router.navigate(['/missions', id]);
  }

  createMission(): void {
    this.router.navigate(['/missions/create']);
  }

  editMission(id: string): void {
    this.router.navigate(['/missions', id, 'edit']);
  }

  /**
   * UC-MSN-09 entry point — §20.S.1's RegisterMission(id) command
   */
  registerResult(id: string): void {
    this.router.navigate(['/missions', id, 'register']);
  }

  /**
   * UC-MSN-08 — confirm first; declining deletes nothing. Raw response: a successful
   * delete returns 200 with no envelope, so there is nothing to map.
   */
  async deleteMission(id: string): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('missions.confirmDelete'),
      this.translate.instant('missions.deleteMission')
    );
    if (!confirmed) return;

    this.missionService.deleteMission(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notification.success(this.translate.instant('missions.missionDeleted'));
          // Deleting the last row of the last page would leave the list on a stale
          // empty page — step back one page before reloading.
          if (this.missions.length === 1 && this.currentPage > 1) {
            this.currentPage--;
          }
          this.loadMissions();
        },
        error: () => {
          this.notification.error(this.translate.instant('missions.deleteFailed'));
        }
      });
  }

  // ========== TrackBys ==========

  trackMission(index: number, mission: Mission): string {
    return mission.id;
  }

  // ========== Pagination ==========

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadMissions();
  }

  /**
   * Export the §20.S.1 register to Excel (charity-list pattern) — the current
   * filters ride along; the server ignores paging and writes every matching row.
   */
  exportToExcel(): void {
    this.loading = true;
    const filters = this.filterForm.value;

    const request: MissionSearchRequest = {
      charityId: filters.charityId && filters.charityId !== MissionListComponent.ALL ? filters.charityId : undefined,
      assignedToUserId: filters.assignedToUserId && filters.assignedToUserId !== MissionListComponent.ALL ? filters.assignedToUserId : undefined,
      search: filters.search?.trim() || undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
      page: 1,
      pageSize: 100000
    };

    this.missionService.exportToExcel(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `missions_${new Date().toISOString().split('T')[0]}.xlsx`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);

          this.notification.success(this.translate.instant('common.operationSuccess'));
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('common.operationFailed'));
        }
      });
  }
}
