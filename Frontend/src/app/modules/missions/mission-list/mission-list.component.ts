/**
 * Mission List Component
 * Displays missions in a grid with filtering, search, and status dashboard
 * Implements UC-8.10 (View Mission List) and UC-8.13 (Track Mission Status)
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import {
  Mission,
  MissionSearchRequest,
  MissionStatusCounts
} from '../models/mission.model';
import { PaginationComponent, BreadcrumbComponent, PageHeaderComponent, BreadcrumbItem } from '../../../shared/components';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-mission-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PaginationComponent, BreadcrumbComponent, PageHeaderComponent],
  templateUrl: './mission-list.component.html',
  styleUrls: ['./mission-list.component.scss']
})
export class MissionListComponent implements OnInit, OnDestroy {
  // Expose Math to template for pagination calculations
  Math = Math;
  private destroy$ = new Subject<void>();

  // Data
  missions: Mission[] = [];
  statusCounts: MissionStatusCounts = {
    pending: 0,
    inProgress: 0,
    completed: 0,
    overdue: 0
  };

  // Loading states
  loading = false;
  statusLoading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Filters
  search = '';
  selectedMissionType?: number;
  selectedMissionTimeType?: number;
  selectedStatus?: string; // 'all', 'pending', 'completed', 'overdue'
  selectedCountry?: number;
  selectedRegion?: number;
  selectedCenter?: number;
  selectedAssignedTo?: string;
  dateFrom?: Date;
  dateTo?: Date;

  // Show my missions only
  showMyMissionsOnly = false;

  // Sort
  sortColumn = 'missionDate';
  sortDirection: 'asc' | 'desc' = 'asc';

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'missions.title' }
  ];

  // Page actions
  pageActions: PageAction[] = [
    {
      label: 'missions.addMission',
      icon: 'fe fe-plus',
      type: 'primary',
      click: () => this.createMission()
    },
    {
      label: 'missions.myMissions',
      icon: 'fe fe-user',
      click: () => this.toggleMyMissions()
    },
    {
      label: 'missions.exportToExcel',
      icon: 'fe fe-download',
      click: () => this.exportToExcel()
    }
  ];

  constructor(
    protected missionService: MissionService,
    protected router: Router,
    protected translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadMissions();
    this.loadStatusCounts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load missions with current filters
   */
  loadMissions(): void {
    this.loading = true;

    const request: MissionSearchRequest = {
      search: this.search || undefined,
      missionTypeId: this.selectedMissionType,
      missionTimeTypeId: this.selectedMissionTimeType,
      isCompleted: this.getCompletedFilter(),
      countryId: this.selectedCountry,
      regionId: this.selectedRegion,
      centerId: this.selectedCenter,
      assignedTo: this.showMyMissionsOnly ? this.getCurrentUserId() : this.selectedAssignedTo,
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
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
        }
      });
  }

  /**
   * Load status counts for dashboard
   */
  loadStatusCounts(): void {
    this.statusLoading = true;

    this.missionService.getMissionStatusCounts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (counts) => {
          this.statusCounts = counts;
          this.statusLoading = false;
        },
        error: () => {
          this.statusLoading = false;
        }
      });
  }

  /**
   * Navigate to mission details
   */
  viewMission(id: string): void {
    this.router.navigate(['/missions', id]);
  }

  /**
   * Navigate to create mission
   */
  createMission(): void {
    this.router.navigate(['/missions/create']);
  }

  /**
   * Navigate to edit mission
   */
  editMission(id: string): void {
    this.router.navigate(['/missions', id, 'edit']);
  }

  /**
   * Mark mission as completed
   */
  markAsCompleted(mission: Mission): void {
    const confirmed = confirm(this.translate.instant('missions.confirmComplete'));
    if (!confirmed) return;

    this.missionService.markAsCompleted(mission.id, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadMissions();
          this.loadStatusCounts();
        },
        error: () => {
          // Error handling
        }
      });
  }

  /**
   * Delete mission
   */
  deleteMission(id: string): void {
    const confirmed = confirm(this.translate.instant('common.deleteConfirm'));
    if (!confirmed) return;

    this.missionService.deleteMission(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadMissions();
          this.loadStatusCounts();
        },
        error: () => {
          // Error handling
        }
      });
  }

  /**
   * Toggle my missions view
   */
  toggleMyMissions(): void {
    this.showMyMissionsOnly = !this.showMyMissionsOnly;
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Apply all filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.search = '';
    this.selectedMissionType = undefined;
    this.selectedMissionTimeType = undefined;
    this.selectedStatus = undefined;
    this.selectedCountry = undefined;
    this.selectedRegion = undefined;
    this.selectedCenter = undefined;
    this.selectedAssignedTo = undefined;
    this.dateFrom = undefined;
    this.dateTo = undefined;
    this.showMyMissionsOnly = false;
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle search
   */
  onSearch(): void {
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle page change
   */
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadMissions();
  }

  /**
   * Handle page size change
   */
  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle sort
   */
  onSort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.loadMissions();
  }

  /**
   * Export missions to Excel
   */
  exportToExcel(): void {
    const request: MissionSearchRequest = {
      search: this.search || undefined,
      missionTypeId: this.selectedMissionType,
      missionTimeTypeId: this.selectedMissionTimeType,
      isCompleted: this.getCompletedFilter(),
      countryId: this.selectedCountry,
      regionId: this.selectedRegion,
      centerId: this.selectedCenter,
      assignedTo: this.showMyMissionsOnly ? this.getCurrentUserId() : this.selectedAssignedTo,
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
      page: 1,
      pageSize: this.totalCount // Export all filtered results
    };

    this.missionService.exportMissions(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `missions_${new Date().toISOString().split('T')[0]}.xlsx`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          // Error handling
        }
      });
  }

  /**
   * Get completed filter value
   */
  private getCompletedFilter(): boolean | undefined {
    if (this.selectedStatus === 'pending') return false;
    if (this.selectedStatus === 'completed') return true;
    return undefined;
  }

  /**
   * Get current user ID (placeholder - should get from AuthService)
   */
  private getCurrentUserId(): string {
    // TODO: Get from AuthService
    return '';
  }

  /**
   * Check if mission is completed
   */
  isCompleted(mission: Mission): boolean {
    return mission.isMissionCompleted;
  }

  /**
   * Check if mission is overdue
   */
  isOverdue(mission: Mission): boolean {
    if (mission.isMissionCompleted) return false;
    const missionDate = new Date(mission.missionDate);
    return missionDate < new Date();
  }

  /**
   * Get mission status class
   */
  getStatusClass(mission: Mission): string {
    if (mission.isMissionCompleted) return 'badge-success';
    if (this.isOverdue(mission)) return 'badge-danger';
    return 'badge-warning';
  }

  /**
   * Get mission status text
   */
  getStatusText(mission: Mission): string {
    if (mission.isMissionCompleted) return this.translate.instant('missions.completed');
    if (this.isOverdue(mission)) return this.translate.instant('missions.overdue');
    return this.translate.instant('missions.pending');
  }

  /**
   * Get page range for pagination
   */
  getPageRange(): number[] {
    // Now using shared pagination component
    return [];
  }
}
