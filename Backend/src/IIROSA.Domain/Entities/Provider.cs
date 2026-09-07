using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Provider entity - Represents non-parent guardian/provider for a family
/// Inherits from FullAuditedEntity which provides all audit fields
/// Implements use case UC-4.6 (Add Non-Parent Provider)
/// </summary>
public class Provider : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Full name of the provider (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Relationship to family: Grandfather, Uncle, Brother, Guardian, Other, etc.
    /// </summary>
    public string RelationshipToFamily { get; set; } = string.Empty;

    /// <summary>
    /// National ID or Passport number (Required)
    /// </summary>
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (Required)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Address of the provider
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Current job/occupation
    /// </summary>
    public string? Job { get; set; }

    /// <summary>
    /// Monthly income amount
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Refugee register extensions (epic 7, UC-REF-03 §12.S.2 اضافة معيل) — all nullable

    /// <summary>
    /// Date of birth (تاريخ الميلاد)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Nationality (الجنسية) — lookup Country
    /// </summary>
    public int? NationalityCountryId { get; set; }

    /// <summary>
    /// Whether the provider is alive (default true); false ⇒ a deceased provider with death details
    /// </summary>
    public bool? IsAlive { get; set; }

    /// <summary>
    /// Date of death, when the provider is deceased
    /// </summary>
    public DateTime? DeathDate { get; set; }

    /// <summary>
    /// Cause of death (سبب الوفاة) — closed set: طبيعية / مرض / حادث (static dropdown, stored as string)
    /// </summary>
    public string? DeathReason { get; set; }

    /// <summary>
    /// Reason of the relation link to the family (السبب) — lookup ReasonOfRel
    /// </summary>
    public int? ReasonOfRelationId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة معيل) — all nullable

    /// <summary>
    /// Relation kind (نوعها) — lookup Relation
    /// </summary>
    public int? RelationId { get; set; }

    /// <summary>
    /// Main relation (العلاقة) — closed set: الاب / الام
    /// </summary>
    public string? MainRelation { get; set; }

    /// <summary>
    /// Social status (الحالة الاجتماعية) — lookup SocialStatus
    /// </summary>
    public int? SocialStatusId { get; set; }

    /// <summary>
    /// Health status (الحالة الصحية) — lookup HealthStatus
    /// </summary>
    public int? HealthStatusId { get; set; }

    /// <summary>
    /// Educational attainment (المؤهل الدراسى) — lookup EducationLevel
    /// </summary>
    public int? EducationLevelId { get; set; }

    /// <summary>
    /// Widow sponsorship flag (كفاله ارمله)
    /// </summary>
    public bool? WidowSponsorship { get; set; }

    /// <summary>
    /// "Mother remarried" flag (الام متزوجة)
    /// </summary>
    public bool? AnotherSponsor { get; set; }

    /// <summary>
    /// "Embraces the orphan" flag (تحتضن اليتيم)
    /// </summary>
    public bool? MotherIsMar { get; set; }

    /// <summary>
    /// "Needs orphan sponsorship" flag (تحتاج كفالة إيتام)
    /// </summary>
    public bool? IsCaring { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
    public virtual Lookups.Country? Country { get; set; }
    public virtual Lookups.ReasonOfRel? ReasonOfRelation { get; set; }
    public virtual Lookups.Relation? Relation { get; set; }
    public virtual Lookups.SocialStatus? SocialStatus { get; set; }
    public virtual Lookups.HealthStatus? HealthStatus { get; set; }
    public virtual Lookups.EducationLevel? EducationLevel { get; set; }
}
