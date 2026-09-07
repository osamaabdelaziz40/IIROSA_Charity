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

    /// <summary>
    /// §14.S.3 grouped rows (UC-ORR-10): one row per
    /// (الحاله التعليميه × المرحله الدراسه) with الاناث/الذكور/الاجمالي counts.
    /// 9-15 extends this response with its numbers-in-period section — do not fork the DTO.
    /// </summary>
    public List<OrphanReportGroupCountRow> Groups { get; set; } = new();

    /// <summary>
    /// §14.U.15 أرقام التقارير المضافة (UC-ORR-15) — the ReportNo values created in the
    /// window, newest first (capped at 1000; <see cref="ReportNumbersCount"/> is uncapped).
    /// Filled only when the request sets <c>IncludeReportNumbers</c> with a date window.
    /// </summary>
    public List<OrphanReportNumberRow> ReportNumbers { get; set; } = new();

    /// <summary>Matching in-window reports before the numbers cap.</summary>
    public int ReportNumbersCount { get; set; }

    /// <summary>In-window reports with a null ReportNo (legacy rows) — listed under a — bucket.</summary>
    public int UnnumberedReportsCount { get; set; }
}

/// <summary>
/// One §14.S.3 grouped-statistics row. The educational-status token is stable
/// (studying/graduated/dropout/unspecified); the level name is resolved
/// server-side (NameAr ?? NameEn), null when the report carries no level.
/// </summary>
public class OrphanReportGroupCountRow
{
    public string EducationalStatus { get; set; } = "unspecified";
    public string? EducationalLevelName { get; set; }
    public int FemaleCount { get; set; }
    public int MaleCount { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// One §14.U.15 numbers-in-period row — the registered number with its orphan identity,
/// dates and review state.
/// </summary>
public class OrphanReportNumberRow
{
    public Guid ReportId { get; set; }
    /// <summary>رقم التقرير — null ReportNo renders under a — bucket (legacy rows).</summary>
    public string? ReportNo { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public string ReviewStatus { get; set; } = "Pending";
}
