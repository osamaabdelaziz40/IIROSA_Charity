import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../../services/periodic-orphan-report.service';
import {
  PeriodicOrphanReportListDto,
  PeriodicOrphanReportFilterDto
} from '../../models/periodic-orphan-report.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-periodic-reports-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    LoadingComponent,
    EmptyStateComponent
  ],
  templateUrl: './periodic-reports-list.component.html',
  styleUrls: ['./periodic-reports-list.component.scss']
})
export class PeriodicReportsListComponent implements OnInit {
  @ViewChild(PaginationComponent) pagination!: PaginationComponent;

  // Tab management - UC-6.14 (Approved), UC-6.15 (Rejected)
  activeTab: 'all' | 'pending' | 'approved' | 'rejected' = 'all';

  // Data
  reports: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;
  loading = false;

  // Filter
  filter: PeriodicOrphanReportFilterDto = {
    pageNumber: 1,
    pageSize: 20,
    sortBy: 'CreatedOn',
    sortDirection: 'DESC'
  };

  // Search
  searchTerm = '';

  // Summary stats
  summaryStats = {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0
  };

  constructor(
    private periodicReportService: PeriodicOrphanReportService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  /**
   * Load reports based on active tab and filters
   */
  loadReports(): void {
    this.loading = true;

    // Set filter based on active tab
    this.filter.pageNumber = 1;
    if (this.pagination) {
      this.filter.pageNumber = this.pagination.currentPage;
    }

    // Apply tab filter
    this.filter.reviewStatus = this.activeTab === 'all' ? undefined : this.activeTab.charAt(0).toUpperCase() + this.activeTab.slice(1);

    this.periodicReportService.getReports(this.filter).subscribe({
      next: (result) => {
        this.reports = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading reports:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Handle tab change - UC-6.14, UC-6.15
   */
  onTabChange(tab: 'all' | 'pending' | 'approved' | 'rejected'): void {
    this.activeTab = tab;
    this.loadReports();
  }

  /**
   * Handle search - UC-6.16
   */
  onSearch(): void {
    this.filter.searchTerm = this.searchTerm;
    this.filter.pageNumber = 1;
    this.loadReports();
  }

  /**
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.filter.searchTerm = undefined;
    this.loadReports();
  }

  /**
   * Handle page change
   */
  onPageChange(page: number): void {
    this.filter.pageNumber = page;
    this.loadReports();
  }

  /**
   * Export reports - UC-6.17
   */
  exportReports(format: 'excel' | 'pdf'): void {
    this.periodicReportService.exportToExcel(this.filter).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, `PeriodicReports_${new Date().toISOString().split('T')[0]}.xlsx`);
      },
      error: (error) => {
        console.error('Error exporting reports:', error);
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
      case 'approved': return 'badge-success';
      case 'rejected': return 'badge-danger';
      case 'pending': return 'badge-warning';
      default: return 'badge-secondary';
    }
  }

  /**
   * Check if report can be edited
   */
  canEditReport(report: PeriodicOrphanReportListDto): boolean {
    return !report.locked && !report.reviewed;
  }

  /**
   * Check if user can review
   */
  get canReview(): boolean {
    // TODO: Check user role (Admin, Super Admin, Accountant, Employee)
    return true;
  }
}
