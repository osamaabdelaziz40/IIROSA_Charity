/**
 * Orphan Report Search Component
 * Implements UC-6.16: Search Orphan Periodic Reports
 *
 * Search and view all periodic reports for specific orphan ordered by
 * creation date (newest first) with:
 * - Visual timeline showing progress over periods
 * - Comparison between periods (side-by-side view)
 * - Export orphan's report history to Excel
 *
 * Access: Charity (own orphans only), Admin/Super Admin (all orphans)
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../../services/periodic-orphan-report.service';
import {
  PeriodicOrphanReportListDto,
  PeriodicOrphanReportSummaryDto
} from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-orphan-report-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    LoadingComponent,
    PaginationComponent,
    EmptyStateComponent
  ],
  templateUrl: './orphan-report-search.component.html',
  styleUrls: ['./orphan-report-search.component.scss']
})
export class OrphanReportSearchComponent implements OnInit {
  // Data
  loading = false;
  reports: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;
  summary?: PeriodicOrphanReportSummaryDto;

  // Search
  searchTerm = '';
  selectedOrphanId?: string;
  currentPage = 1;
  pageSize = 10;

  // Orphans list (would be populated from service)
  orphans: any[] = [];

  // View mode
  viewMode: 'list' | 'timeline' = 'list';

  // Comparison mode
  comparisonMode = false;
  selectedReportIds: string[] = [];

  // User role
  isAdminOrSuperAdmin = false; // Would come from auth service

  constructor(
    private periodicReportService: PeriodicOrphanReportService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if orphan ID is in route - UC-6.16
    const orphanId = this.route.snapshot.params['orphanId'];
    if (orphanId) {
      this.selectedOrphanId = orphanId;
      this.loadOrphanReports(orphanId);
    }

    // Load orphans list for dropdown
    this.loadOrphans();
  }

  /**
   * Load orphans list
   */
  private loadOrphans(): void {
    // TODO: Load from appropriate service
    // This.orphans = this.orphanService.getAll();
  }

  /**
   * Load orphan's periodic reports - UC-6.16
   */
  private loadOrphanReports(orphanId: string): void {
    this.loading = true;

    this.periodicReportService.getReportsByOrphan(orphanId, this.currentPage, this.pageSize).subscribe({
      next: (result) => {
        // Order by TimeStamp DESC (newest first) - UC-6.16
        this.reports = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading orphan reports:', error);
        this.loading = false;
      }
    });

    // Load summary - UC-6.16
    this.periodicReportService.getOrphanReportSummary(orphanId).subscribe({
      next: (summary) => {
        this.summary = summary;
      },
      error: (error) => {
        console.error('Error loading summary:', error);
      }
    });
  }

  /**
   * Search by orphan name or ID - UC-6.16
   */
  onSearch(): void {
    if (!this.searchTerm && !this.selectedOrphanId) {
      return;
    }

    // If orphan selected, load their reports
    if (this.selectedOrphanId) {
      this.loadOrphanReports(this.selectedOrphanId);
      return;
    }

    // Otherwise, search by term
    // This.filter.searchTerm = this.searchTerm;
    // This.loadReports();
  }

  /**
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.selectedOrphanId = undefined;
    this.reports = [];
    this.totalCount = 0;
    this.summary = undefined;
  }

  /**
   * Handle page change
   */
  onPageChange(page: number): void {
    this.currentPage = page;
    if (this.selectedOrphanId) {
      this.loadOrphanReports(this.selectedOrphanId);
    }
  }

  /**
   * View report details - UC-6.16
   */
  viewReport(reportId: string): void {
    this.router.navigate(['/periodic-reports', reportId]);
  }

  /**
   * Export orphan's report history to Excel - UC-6.16, UC-6.17
   */
  exportHistory(orphanId?: string): void {
    const id = orphanId || this.selectedOrphanId;
    if (!id) return;

    this.periodicReportService.exportOrphanHistoryToExcel(id).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, `OrphanReportHistory_${id}_${new Date().toISOString().split('T')[0]}.xlsx`);
      },
      error: (error) => {
        console.error('Error exporting history:', error);
      }
    });
  }

  /**
   * Toggle comparison mode
   */
  toggleComparison(): void {
    this.comparisonMode = !this.comparisonMode;
    this.selectedReportIds = [];
  }

  /**
   * Toggle report selection for comparison
   */
  toggleReportSelection(reportId: string): void {
    const index = this.selectedReportIds.indexOf(reportId);
    if (index > -1) {
      this.selectedReportIds.splice(index, 1);
    } else if (this.selectedReportIds.length < 2) {
      this.selectedReportIds.push(reportId);
    }
  }

  /**
   * Compare selected reports
   */
  compareReports(): void {
    if (this.selectedReportIds.length !== 2) {
      alert('Please select exactly 2 reports to compare');
      return;
    }

    // Navigate to comparison view
    this.router.navigate(['/orphan-reports/compare'], {
      queryParams: {
        report1: this.selectedReportIds[0],
        report2: this.selectedReportIds[1]
      }
    });
  }

  /**
   * Download file helper
   */
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Get status badge class
   */
  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'approved':
      case 'accepted':
        return 'bg-success';
      case 'rejected':
      case 'refused':
        return 'bg-danger';
      case 'pending':
        return 'bg-warning';
      default:
        return 'bg-secondary';
    }
  }

  /**
   * Get orphan info
   */
  get orphanInfo(): any {
    if (this.selectedOrphanId) {
      return this.orphans.find(o => o.id === this.selectedOrphanId);
    }
    return null;
  }

  /**
   * Check if report is selected for comparison
   */
  isReportSelected(reportId: string): boolean {
    return this.selectedReportIds.includes(reportId);
  }

  /**
   * Get progress percentage based on reports
   */
  getProgressPercentage(report: PeriodicOrphanReportListDto): number {
    // Simple progress based on report count
    if (this.totalCount === 0) return 0;
    const index = this.reports.indexOf(report);
    return Math.round(((this.totalCount - index) / this.totalCount) * 100);
  }
}
