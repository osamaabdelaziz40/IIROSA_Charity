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
    /// Filter by orphan code (كود اليتيم) — §14.S.1 dedicated filter
    /// </summary>
    public string? OrphanCode { get; set; }

    /// <summary>
    /// Filter by orphan name (اسم اليتيم) — §14.S.1 dedicated filter
    /// </summary>
    public string? OrphanName { get; set; }

    /// <summary>
    /// Filter by specific orphan - UC-6.16
    /// </summary>
    public Guid? OrphanId { get; set; }

    /// <summary>
    /// UC-HOU-06 (§11.S.3) — the ChildOrParent discriminator: Child (family's children) or
    /// Parent (the housing family's guardian). Null = both (the epic-9 default).
    /// </summary>
    public Domain.Enums.ReportBeneficiaryType? ChildOrParent { get; set; }

    /// <summary>
    /// UC-HOU-06 (§11.S.3) — the housing family whose reports are listed (guardian-subject
    /// reports are linked through this, not through OrphanId).
    /// </summary>
    public Guid? HousingFamilyId { get; set; }

    /// <summary>
    /// Filter by charity (Admin/Super Admin only) - UC-6.14, UC-6.15
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// رقم التقرير — dedicated partial-match filter (UC-ORR-12/13 extract criterion);
    /// SearchTerm above also matches it, but the extract criteria name it alone.
    /// </summary>
    public string? ReportNo { get; set; }

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

    // ---- §14.S.4 orphan-status search (UC-ORR-09) --------------------------------
    // The status family below combines with && or || per <see cref="AndOr"/>; the
    // base filters above (scope, charity, dates, orphan) always AND.

    /// <summary>
    /// How the §14.S.4 status predicates combine: "and" (default) = all must hold,
    /// "or" = any. The legacy contract's only surviving name (OrphanStatusRefinedContract).
    /// </summary>
    public string? AndOr { get; set; }

    /// <summary>
    /// نوع التعليم — matches the stored free-text value (حكومي / أهلي)
    /// </summary>
    public string? SchoolType { get; set; }

    /// <summary>
    /// الحالة الاجتماعية — tri-state token: married / single / deceased,
    /// mapping to the report's Married / Dead flags
    /// </summary>
    public string? MaritalStatus { get; set; }

    /// <summary>
    /// الحالة التعليمية — tri-state token: studying / graduated / dropout,
    /// mapping to IsOrphanStudent / HighestEducationalLevel / DropOut
    /// </summary>
    public string? EducationalStatus { get; set; }

    /// <summary>
    /// اخر تقدير — matches the stored free-text grade (ممتاز / جيد جدا / جيد / مقبول / ضعيف)
    /// </summary>
    public string? EducationDegree { get; set; }

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
