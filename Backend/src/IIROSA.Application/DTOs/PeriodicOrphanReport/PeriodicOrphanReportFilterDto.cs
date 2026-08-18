namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Filter DTO for Periodic Orphan Reports - Implements filtering from UC-6.14, UC-6.15, UC-6.16
/// </summary>
public class PeriodicOrphanReportFilterDto
{
    /// <summary>
    /// Search by orphan name or report number - UC-6.14, UC-6.15, UC-6.16
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by specific orphan - UC-6.16
    /// </summary>
    public Guid? OrphanId { get; set; }

    /// <summary>
    /// Filter by charity (Admin/Super Admin only) - UC-6.14, UC-6.15
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// Filter by review status - UC-6.14 (Approved), UC-6.15 (Rejected)
    /// Values: "Pending", "Approved", "Rejected", "All"
    /// </summary>
    public string? ReviewStatus { get; set; }

    /// <summary>
    /// Show only reviewed reports
    /// </summary>
    public bool? Reviewed { get; set; }

    /// <summary>
    /// Show only accepted reports - UC-6.14
    /// </summary>
    public bool? IsAccepted { get; set; }

    /// <summary>
    /// Show only refused reports - UC-6.15
    /// </summary>
    public bool? IsRefused { get; set; }

    /// <summary>
    /// Filter by reviewer - UC-6.14, UC-6.15
    /// </summary>
    public Guid? ReviewerId { get; set; }

    /// <summary>
    /// Filter by report date range - UC-6.14, UC-6.15
    /// </summary>
    public DateTime? ReportDateFrom { get; set; }
    public DateTime? ReportDateTo { get; set; }

    /// <summary>
    /// Filter by educational stage
    /// </summary>
    public int? EducationalStageId { get; set; }

    /// <summary>
    /// Filter by educational level
    /// </summary>
    public int? EducationalLevelId { get; set; }

    /// <summary>
    /// Filter by medical status
    /// </summary>
    public string? MedicalStatus { get; set; }

    /// <summary>
    /// Show only active reports
    /// </summary>
    public bool? Active { get; set; } = true;

    /// <summary>
    /// Show only locked reports
    /// </summary>
    public bool? Locked { get; set; }

    /// <summary>
    /// Pagination - Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Pagination - Page size
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sorting field
    /// </summary>
    public string? SortBy { get; set; } = "CreatedOn";

    /// <summary>
    /// Sort direction (ASC or DESC)
    /// </summary>
    public string? SortDirection { get; set; } = "DESC";
}
