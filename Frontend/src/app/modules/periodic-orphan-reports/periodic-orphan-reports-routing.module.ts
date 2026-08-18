/**
 * Periodic Orphan Reports Routing Module
 * Route definitions for periodic orphan reports functionality
 *
 * Access Control:
 * - Charity users: Can only access their own orphan reports and periodic reports for their orphans
 * - Admin/Super Admin: Full access to all reports, including orphan reports generation
 * - Accountant/Employee: Can review periodic reports
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Periodic Reports Components
import { PeriodicReportsListComponent } from './periodic-reports-list/periodic-reports-list.component';
import { PeriodicReportFormComponent } from './periodic-report-form/periodic-report-form.component';
import { PeriodicReportDetailComponent } from './periodic-report-detail/periodic-report-detail.component';
import { PeriodicReportReviewComponent } from './periodic-report-review/periodic-report-review.component';

// Orphan Reports Components
import { OrphanReportsListComponent } from './orphan-reports-list/orphan-reports-list.component';
import { OrphanReportsGenerateComponent } from './orphan-reports-generate/orphan-reports-generate.component';
import { OrphanReportHistoryComponent } from './orphan-report-history/orphan-report-history.component';
import { OrphanReportComparisonComponent } from './orphan-report-comparison/orphan-report-comparison.component';
import { ScheduleReportComponent } from './schedule-report/schedule-report.component';
import { OrphanReportSearchComponent } from './orphan-report-search/orphan-report-search.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const periodicOrphanReportsRoutes: Routes = [
  // ==================== PERIODIC ORPHAN REPORTS ====================
  // Main list with tabs for All/Pending/Approved/Rejected - UC-6.14, UC-6.15
  {
    path: '',
    component: PeriodicReportsListComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'periodicReports.title',
      breadcrumb: 'periodicReports.breadcrumb.list'
    }
  },

  // Create new periodic report - UC-6.11
  {
    path: 'create',
    component: PeriodicReportFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'periodicReports.createReport',
      breadcrumb: 'periodicReports.breadcrumb.create'
    }
  },

  // View periodic report details
  {
    path: ':id',
    component: PeriodicReportDetailComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'periodicReports.reportDetails',
      breadcrumb: 'periodicReports.breadcrumb.details'
    }
  },

  // Edit periodic report - UC-6.11
  {
    path: ':id/edit',
    component: PeriodicReportFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'periodicReports.editReport',
      breadcrumb: 'periodicReports.breadcrumb.edit'
    }
  },

  // Review periodic report (approve/reject) - UC-6.13
  {
    path: ':id/review',
    component: PeriodicReportReviewComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.reviewReport',
      breadcrumb: 'periodicReports.breadcrumb.review',
      permission: 'PeriodicReports.Review',
      roles: ['Admin', 'SuperAdmin', 'Accountant', 'Employee']
    }
  },

  // ==================== ORPHAN REPORTS ====================
  // Orphan Reports list/generation - UC-6.1
  {
    path: 'orphan-reports',
    component: OrphanReportsListComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.title',
      breadcrumb: 'orphanReports.breadcrumb.list'
    }
  },

  // Generate orphan report with filters - UC-6.1
  {
    path: 'orphan-reports/generate',
    component: OrphanReportsGenerateComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.generate',
      breadcrumb: 'orphanReports.breadcrumb.generate'
    }
  },

  // Orphan Report History - UC-6.9
  {
    path: 'orphan-reports/history',
    component: OrphanReportHistoryComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.history',
      breadcrumb: 'orphanReports.breadcrumb.history'
    }
  },

  // Compare orphan reports - UC-6.10
  {
    path: 'orphan-reports/compare',
    component: OrphanReportComparisonComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'orphanReports.compare',
      breadcrumb: 'orphanReports.breadcrumb.compare',
      permission: 'OrphanReports.Compare',
      roles: ['Admin', 'SuperAdmin']
    }
  },

  // Schedule recurring report - UC-6.8
  {
    path: 'orphan-reports/schedule',
    component: ScheduleReportComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.schedule',
      breadcrumb: 'orphanReports.breadcrumb.schedule'
    }
  },

  // Search orphan's periodic reports - UC-6.16
  {
    path: 'orphan-reports/search',
    component: OrphanReportSearchComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.search',
      breadcrumb: 'orphanReports.breadcrumb.search'
    }
  },

  // Search by orphan ID shortcut - UC-6.16
  {
    path: 'orphan-reports/orphan/:orphanId',
    component: OrphanReportSearchComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'orphanReports.orphanReports',
      breadcrumb: 'orphanReports.breadcrumb.orphanReports'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(periodicOrphanReportsRoutes)],
  exports: [RouterModule]
})
export class PeriodicOrphanReportsRoutingModule { }
