namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Periodic Orphan Report List DTO - For grid displays - UC-6.14, UC-6.15, UC-6.16
/// </summary>
public class PeriodicOrphanReportListDto
{
    public Guid Id { get; set; }

    /// <summary>UC-HOU-06 (§11.S.3): which kind of housing beneficiary this report is for —
    /// "Child" or "Parent" (guardian report riding a carrier child's OrphanId).</summary>
    public string? ChildOrParent { get; set; }

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
    public int? EducationalLevelId { get; set; }
    public string? EducationalLevelName { get; set; }
    public string? MedicalStatus { get; set; }

    // Status Information - UC-6.14, UC-6.15
    public bool Reviewed { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsRefused { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedDate { get; set; }

    // Refusal evidence — the correction worklist columns (UC-ORR-13, epic 9)
    public int? RefuseReasonId { get; set; }
    public string? RefuseReason { get; set; }
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Computed review status for filtering and display
    /// </summary>
    public string ReviewStatus { get; set; } = "Pending";

    // Life Events flags for quick reference
    public bool? Married { get; set; }
    public bool? Dead { get; set; }

    // ---- §14.S.4 orphan-status wide grid (UC-ORR-09) ----------------------------
    // Flattened Orphan → Family (→ Provider / Region / Center) plus the
    // report-dimension columns. Nullable: the register grid (§14.S.1) simply
    // ignores them; columns whose source field does not exist on this stack
    // (مشروع تنموى للمعيل, الاستبعاد pair) render "—" client-side.

    // Orphan dimension
    public DateTime? OrphanDateOfBirth { get; set; }
    public string? OrphanGender { get; set; }
    public string? OrphanNationalId { get; set; }
    public string? OrphanPhone { get; set; }

    // Family / address dimension
    public string? FamilyCode { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianNationalId { get; set; }
    public string? GuardianJob { get; set; }
    public string? GuardianEducationLevelName { get; set; }
    public string? RegionName { get; set; }
    public string? CenterName { get; set; }
    public string? CityVillage { get; set; }
    public string? Address { get; set; }
    public string? HomePhone { get; set; }

    // Report dimension
    public string? SchoolType { get; set; }
    public string? Faculty { get; set; }
    public string? School { get; set; }
    public DateTime? MarriageDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? Disease { get; set; }
    public string? Disability { get; set; }
    public string? Grade { get; set; }
    public string? Specialization { get; set; }
    public string? EducationDegree { get; set; }
    public DateTime? UpdatedOn { get; set; }

    // Timestamp for ordering - UC-6.16
    public DateTime CreatedOn { get; set; }
}
