namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Report counters for one orphan — the summary that rides the report-history view (UC-ORR-06/16, epic 9).
/// </summary>
public class PeriodicOrphanReportSummaryDto
{
    public Guid OrphanId { get; set; }
    public string? OrphanName { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }
    public int RejectedReports { get; set; }
    public int LockedReports { get; set; }
}
