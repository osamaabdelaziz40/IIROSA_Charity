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
import { OrphanReportStateExtractComponent } from './orphan-report-state-extract/orphan-report-state-extract.component';
import { NonRenewedReportsComponent } from './non-renewed-reports/non-renewed-reports.component';
import { ReportNumbersComponent } from './report-numbers/report-numbers.component';
import { PeriodicReportPrintComponent } from './periodic-report-print/periodic-report-print.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const periodicOrphanReportsRoutes: Routes = [
  // ==================== PERIODIC ORPHAN REPORTS ====================
  // Register list §14.S.1 - UC-ORR-01 (tabs are not in the spec; status filtering is 9-9)
  {
    path: '',
    component: PeriodicReportsListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.title',
      breadcrumb: 'periodicReports.breadcrumb.list',
      permission: 'PeriodicReports.View'
    }
  },

  // Create new periodic report - UC-6.11
  {
    path: 'create',
    component: PeriodicReportFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.createReport',
      breadcrumb: 'periodicReports.breadcrumb.create',
      permission: 'PeriodicReports.Create'
    }
  },

  // ==================== ORPHAN REPORTS ====================
  // §14.S.3 grouped statistics — UC-ORR-10
  // Review P22 2026-08-24 (CRITICAL): this single-segment route was declared AFTER the
  // ':id' route below, so Angular matched 'orphan-reports' as a detail id and the
  // statistics screen was unreachable via the sidebar. Static segments must be
  // declared before the ':id' catch-all (first-declared wins).
  {
    path: 'orphan-reports',
    component: OrphanReportsListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'orphanReports.statistics.title',
      breadcrumb: 'orphanReports.statistics.title',
      permission: 'PeriodicReports.View'
    }
  },

  // View periodic report details
  {
    path: ':id',
    component: PeriodicReportDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.reportDetails',
      breadcrumb: 'periodicReports.breadcrumb.details',
      permission: 'PeriodicReports.View'
    }
  },

  // Edit periodic report - UC-6.11
  {
    path: ':id/edit',
    component: PeriodicReportFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.editReport',
      breadcrumb: 'periodicReports.breadcrumb.edit',
      permission: 'PeriodicReports.Edit'
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

  // §14.U.17 print the periodic report form — UC-ORR-17 (client-side print ruling)
  {
    path: ':id/print',
    component: PeriodicReportPrintComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.print.title',
      breadcrumb: 'periodicReports.print.title',
      permission: 'PeriodicReports.View'
    }
  },

  // §14.U.11 extract detailed report data — UC-ORR-11
  {
    path: 'orphan-reports/generate',
    component: OrphanReportsGenerateComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'orphanReports.generate.title',
      breadcrumb: 'orphanReports.generate.title',
      permission: 'PeriodicReports.View'
    }
  },

  // §14.U.12 accepted / §14.U.13 refused extracts — shared state-filtered screen.
  // Review AA3 2026-08-24: two literal routes give each tab its own translated title;
  // the generic ':state' fallback stays last for deep links, and the component guards
  // unknown states (9-13) instead of silently rendering the accepted extract.
  {
    path: 'orphan-reports/extract/accepted',
    component: OrphanReportStateExtractComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.extract.accepted.title',
      breadcrumb: 'periodicReports.extract.accepted.title',
      permission: 'PeriodicReports.View'
    }
  },
  {
    path: 'orphan-reports/extract/refused',
    component: OrphanReportStateExtractComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.extract.refused.title',
      breadcrumb: 'periodicReports.extract.refused.title',
      permission: 'PeriodicReports.View'
    }
  },
  {
    path: 'orphan-reports/extract/:state',
    component: OrphanReportStateExtractComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.extract.accepted.title',
      breadcrumb: 'periodicReports.extract.accepted.title',
      permission: 'PeriodicReports.View'
    }
  },

  // §14.U.14 non-renewed chase list — UC-ORR-14
  {
    path: 'orphan-reports/non-renewed',
    component: NonRenewedReportsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.nonRenewed.title',
      breadcrumb: 'periodicReports.nonRenewed.title',
      permission: 'PeriodicReports.View'
    }
  },

  // §14.U.15 report numbers added in a period — UC-ORR-15
  {
    path: 'orphan-reports/report-numbers',
    component: ReportNumbersComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.reportNumbers.title',
      breadcrumb: 'periodicReports.reportNumbers.title',
      permission: 'PeriodicReports.View'
    }
  },

  // Orphan Report History - UC-6.9
  // Review P23 2026-08-24: AuthGuard alone is not an authorisation control — the
  // sidebar hides this entry behind PeriodicReports.View, so the route must enforce
  // the same permission server-side-of-the-router (CLAUDE.md: no client-only authz).
  {
    path: 'orphan-reports/history',
    component: OrphanReportHistoryComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'orphanReports.history',
      breadcrumb: 'orphanReports.breadcrumb.history',
      permission: 'PeriodicReports.View'
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
  // Review P23 2026-08-24: same as history — permission-enforced, not menu-hidden only.
  {
    path: 'orphan-reports/schedule',
    component: ScheduleReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'orphanReports.schedule',
      breadcrumb: 'orphanReports.breadcrumb.schedule',
      permission: 'PeriodicReports.View'
    }
  },

  // §14.S.4 orphan-status search — UC-ORR-09
  {
    path: 'orphan-reports/search',
    component: OrphanReportSearchComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.search.title',
      breadcrumb: 'periodicReports.search.title',
      permission: 'PeriodicReports.View'
    }
  },

  // Search by orphan ID shortcut — same screen, prefilled subject
  {
    path: 'orphan-reports/orphan/:orphanId',
    component: OrphanReportSearchComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'periodicReports.search.title',
      breadcrumb: 'periodicReports.search.title',
      permission: 'PeriodicReports.View'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(periodicOrphanReportsRoutes)],
  exports: [RouterModule]
})
export class PeriodicOrphanReportsRoutingModule { }
