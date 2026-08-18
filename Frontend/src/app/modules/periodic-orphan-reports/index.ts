/**
 * Periodic Orphan Reports Module - Public API
 *
 * This module provides comprehensive functionality for:
 * - Orphan Reports generation with filters (UC-6.1 through UC-6.10)
 * - Periodic orphan report creation and management (UC-6.11 through UC-6.17)
 *
 * Main Exports:
 * - PeriodicOrphanReportsModule: The main NgModule
 * - PeriodicOrphanReportsRoutingModule: Route definitions
 *
 * Services:
 * - PeriodicOrphanReportService: Handles periodic report CRUD and review operations
 * - OrphanReportService: Handles orphan report generation, export, history, comparison, and scheduling
 *
 * Components:
 * - PeriodicReportsListComponent: Main list with tabs (All/Pending/Approved/Rejected)
 * - PeriodicReportFormComponent: Create/edit periodic reports
 * - PeriodicReportDetailComponent: View comprehensive report details
 * - PeriodicReportReviewComponent: Review and approve/reject reports
 * - OrphanReportsListComponent: Entry point for orphan reports
 * - OrphanReportsGenerateComponent: Generate reports with filters
 * - OrphanReportHistoryComponent: View report history
 * - OrphanReportComparisonComponent: Compare two periods
 * - ScheduleReportComponent: Schedule recurring reports
 * - OrphanReportSearchComponent: Search orphan-specific reports
 */

export { PeriodicOrphanReportsModule } from './periodic-orphan-reports.module';
export { PeriodicOrphanReportsRoutingModule } from './periodic-orphan-reports-routing.module';

// Services
export { PeriodicOrphanReportService } from './services/periodic-orphan-report.service';
export { OrphanReportService } from './services/orphan-report.service';

// Models
export * from './models/periodic-orphan-report.model';

// Components (for lazy loading or direct imports)
export { PeriodicReportsListComponent } from './periodic-reports-list/periodic-reports-list.component';
export { PeriodicReportFormComponent } from './periodic-report-form/periodic-report-form.component';
export { PeriodicReportDetailComponent } from './periodic-report-detail/periodic-report-detail.component';
export { PeriodicReportReviewComponent } from './periodic-report-review/periodic-report-review.component';
export { OrphanReportsListComponent } from './orphan-reports-list/orphan-reports-list.component';
export { OrphanReportsGenerateComponent } from './orphan-reports-generate/orphan-reports-generate.component';
export { OrphanReportHistoryComponent } from './orphan-report-history/orphan-report-history.component';
export { OrphanReportComparisonComponent } from './orphan-report-comparison/orphan-report-comparison.component';
export { ScheduleReportComponent } from './schedule-report/schedule-report.component';
export { OrphanReportSearchComponent } from './orphan-report-search/orphan-report-search.component';
