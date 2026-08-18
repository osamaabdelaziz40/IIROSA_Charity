namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Periodic Orphan Report List DTO - For grid displays - UC-6.14, UC-6.15, UC-6.16
/// </summary>
public class PeriodicOrphanReportListDto
{
    public Guid Id { get; set; }
    public string? ReportNo { get; set; }
    public DateTime ReportDate { get; set; }
    public DateTime? ReportPeriodFrom { get; set; }
    public DateTime? ReportPeriodTo { get; set; }

    // Orphan Information - UC-6.16
    public Guid OrphanId { get; set; }
    public string? OrphanCode { get; set; }
    public string? OrphanName { get; set; }

    // Charity Information - UC-6.14, UC-6.15
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Key Progress Fields for Summary View
    public string? PrayerStatus { get; set; }
    public string? EducationalLevelName { get; set; }
    public string? MedicalStatus { get; set; }

    // Status Information - UC-6.14, UC-6.15
    public bool Reviewed { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsRefused { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedDate { get; set; }

    /// <summary>
    /// Computed review status for filtering and display
    /// </summary>
    public string ReviewStatus { get; set; } = "Pending";

    // Life Events flags for quick reference
    public bool? Married { get; set; }
    public bool? Dead { get; set; }

    // Timestamp for ordering - UC-6.16
    public DateTime CreatedOn { get; set; }
}
