# Periodic Orphan Reports Module

## Overview
This module implements the Periodic Orphan Reports functionality for the IIROSA Charities application, covering Use Cases UC-6.1 through UC-6.17 from the use case specification.

## Features Implemented

### Orphan Reports (UC-6.1 through UC-6.10)
- **Report Generation (UC-6.1)**: Comprehensive form with filters for period, charity, region, center, sponsorship status, age range, and gender
- **Report Period Selection (UC-6.2)**: Date range picker for specifying report period
- **Sponsorship Status Filter (UC-6.3)**: Filter by Sponsored, Unsponsored, Pending, or All
- **Charity Filter (UC-6.4)**: Admin/Super Admin only dropdown for charity selection
- **Region Filter (UC-6.5)**: Filter reports by geographic region
- **Family Details Option (UC-6.6)**: Checkbox to include family information
- **Report Export (UC-6.7)**: Export to Excel and PDF formats
- **Recurring Schedule (UC-6.8)**: Configure automatic periodic report generation
- **Report History (UC-6.9)**: View previously generated reports
- **Period Comparison (UC-6.10)**: Compare statistics between two periods (Admin/Super Admin only)

### Periodic Orphan Reports (UC-6.11 through UC-6.17)
- **Create Periodic Report (UC-6.11)**: Comprehensive form with all progress tracking sections
  - Religious & Behavioral (Prayer, Manners, Hadeeth status)
  - Quran Education (Parts, Verses)
  - Health & Medical (Medical status, diseases, disabilities)
  - Personal Development (Hobbies, courses, achievements, wishes)
  - Education Details (School, grade, faculty, specialization)
  - Life Events (Marriage, death)
  - Attachments (Medical report, certificate, orphan image)
- **Enable for Charity (UC-6.12)**: Settings configuration (Admin only)
- **Review Report (UC-6.13)**: Approve or reject with refusal reasons
- **View Approved Reports (UC-6.14)**: Filtered list with tabs
- **View Rejected Reports (UC-6.15)**: List with refusal reasons
- **Search Orphan Reports (UC-6.16)**: Search by orphan with timeline view
- **Export to Excel (UC-6.17)**: Export all data fields

## Component Structure

```
periodic-orphan-reports/
├── period-reports-list/          # Main list with tabs
├── periodic-report-form/          # Create/edit periodic reports
├── periodic-report-detail/        # View report details
├── periodic-report-review/        # Review and approve/reject
├── orphan-reports-list/           # Entry point for orphan reports
├── orphan-reports-generate/       # Generate reports with filters
├── orphan-report-history/         # View report history
├── orphan-report-comparison/      # Compare two periods
├── schedule-report/               # Schedule recurring reports
├── orphan-report-search/          # Search orphan-specific reports
├── models/                        # TypeScript interfaces
├── services/                      # API services
└── periodic-orphan-reports.module.ts
```

## Access Control

- **Charity Users**: Can only access their own orphans' data
- **Admin/Super Admin**: Full access to all reports and features
- **Accountant/Employee**: Can review periodic reports

## Usage

### Register the Module
```typescript
import { PeriodicOrphanReportsModule } from './modules/periodic-orphan-reports';

@NgModule({
  imports: [
    PeriodicOrphanReportsModule,
    // ...
  ]
})
export class AppModule { }
```

### Add Routes to App Router
```typescript
{
  path: 'periodic-reports',
  loadChildren: () => import('./modules/periodic-orphan-reports')
    .then(m => m.PeriodicOrphanReportsModule)
}
```

## Services

### PeriodicOrphanReportService
Handles periodic orphan report CRUD operations:
- `createReport()`: Create new periodic report
- `getReport()`: Get report by ID
- `updateReport()`: Update existing report
- `deleteReport()`: Delete report
- `reviewReport()`: Approve/reject report
- `getReports()`: List with filtering
- `getApprovedReports()`: Get approved reports only
- `getRejectedReports()`: Get rejected reports only
- `getReportsByOrphan()`: Get reports for specific orphan
- `exportToExcel()`: Export to Excel

### OrphanReportService
Handles orphan summary reports:
- `generateReport()`: Generate orphan report with filters
- `exportReport()`: Export to Excel/PDF
- `getReportHistory()`: View generated reports
- `comparePeriods()`: Compare two report periods
- `scheduleRecurringReport()`: Schedule automatic generation
- `getScheduledReports()`: List scheduled reports

## Dependencies
- Angular Common, Forms, RouterModule
- ngx-translate for internationalization
- Shared components (Loading, Pagination, EmptyState, PageHeader, Breadcrumb)

## Translation Keys
The module uses the following translation key prefixes:
- `periodicReports.*`: For periodic orphan reports
- `orphanReports.*`: For orphan summary reports
- `common.*`: For shared labels (buttons, actions, etc.)

## Future Enhancements
- Implement visual charts/graphs for comparison view
- Add email notification for scheduled reports
- Implement audit log viewer
- Add bulk approval/rejection functionality
- Export to additional formats (CSV, Word)
