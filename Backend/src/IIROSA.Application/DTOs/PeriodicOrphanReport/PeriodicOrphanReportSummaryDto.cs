namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Summary statistics for Periodic Orphan Reports
/// </summary>
public class PeriodicOrphanReportSummaryDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }
    public int RejectedReports { get; set; }
    public int LockedReports { get; set; }

    // Orphan-specific summary - UC-6.16
    public int OrphanId { get; set; }
    public string? OrphanName { get; set; }
    public int TotalReportsForOrphan { get; set; }
}
