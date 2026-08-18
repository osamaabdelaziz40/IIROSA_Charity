using IIROSA.Application.DTOs.OrphanReport;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Orphan Report Service Interface
/// Implements use cases UC-6.1 through UC-6.10 for Orphan Summary Reports
/// </summary>
public interface IOrphanReportService
{
    #region Report Generation (UC-6.1)

    /// <summary>
    /// Generate orphan report with filters - UC-6.1
    /// Charity users see only their orphans
    /// Admin/Super Admin see all orphans with optional charity filter
    /// </summary>
    Task<OrphanReportResultDto> GenerateReportAsync(OrphanReportFilterDto filter);

    #endregion

    #region Report Period (UC-6.2)

    /// <summary>
    /// Validate report period (From <= To) - UC-6.2
    /// </summary>
    Task<bool> ValidateReportPeriodAsync(DateTime fromDate, DateTime toDate);

    #endregion

    #region Export Operations (UC-6.7)

    /// <summary>
    /// Export orphan report to Excel or PDF - UC-6.7
    /// </summary>
    Task<byte[]> ExportReportAsync(OrphanReportFilterDto filter, OrphanReportExportDto exportOptions);

    #endregion

    #region Report History (UC-6.9)

    /// <summary>
    /// View report history - UC-6.9
    /// Charity users see only their reports
    /// Admin/Super Admin see all reports
    /// </summary>
    Task<(IEnumerable<OrphanReportHistoryDto> Items, int TotalCount)> GetReportHistoryAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get report history entry by ID
    /// </summary>
    Task<OrphanReportHistoryDto?> GetReportHistoryEntryAsync(Guid reportId);

    /// <summary>
    /// Delete report from history
    /// </summary>
    Task DeleteReportHistoryEntryAsync(Guid reportId);

    #endregion

    #region Period Comparison (UC-6.10)

    /// <summary>
    /// Compare two report periods - UC-6.10
    /// Admin/Super Admin only
    /// </summary>
    Task<OrphanReportComparisonDto> ComparePeriodsAsync(Guid report1Id, Guid report2Id);

    #endregion

    #region Scheduling (UC-6.8)

    /// <summary>
    /// Schedule recurring report - UC-6.8
    /// </summary>
    Task<Guid> ScheduleRecurringReportAsync(ScheduleRecurringReportDto dto);

    /// <summary>
    /// Get scheduled reports
    /// </summary>
    Task<(IEnumerable<ScheduledReportDto> Items, int TotalCount)> GetScheduledReportsAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Update scheduled report
    /// </summary>
    Task UpdateScheduledReportAsync(Guid scheduleId, ScheduleRecurringReportDto dto);

    /// <summary>
    /// Delete scheduled report
    /// </summary>
    Task DeleteScheduledReportAsync(Guid scheduleId);

    #endregion

    #region Statistics

    /// <summary>
    /// Get orphan statistics for dashboard
    /// </summary>
    Task<OrphanStatisticsDto> GetOrphanStatisticsAsync(OrphanReportFilterDto filter);

    #endregion
}

/// <summary>
/// DTO for scheduling recurring reports - UC-6.8
/// </summary>
public class ScheduleRecurringReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public string Frequency { get; set; } = "Monthly"; // Monthly, Quarterly, Annually
    public int? DayOfMonth { get; set; }
    public string EmailRecipients { get; set; } = string.Empty; // Comma-separated emails
    public OrphanReportFilterDto Filter { get; set; } = new();
}

/// <summary>
/// DTO for scheduled report - UC-6.8
/// </summary>
public class ScheduledReportDto
{
    public Guid ScheduleId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int? DayOfMonth { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;
    public DateTime? NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Orphan statistics for dashboard
/// </summary>
public class OrphanStatisticsDto
{
    public int TotalOrphans { get; set; }
    public int SponsoredCount { get; set; }
    public int UnsponsoredCount { get; set; }
    public int PendingCount { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
}
