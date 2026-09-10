using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Mother entity - Represents mother of a family
/// Inherits from FullAuditedEntity which provides all audit fields
/// Implements use cases UC-4.3 (Add Family Mother) and UC-4.10 (Update Mother Details)
/// </summary>
public class Mother : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Full name of the mother (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// First name (الاسم الأول) — legacy WAR.IIROSA four-part name, §10 اضافة معيل
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Second name (الاسم الثانى)
    /// </summary>
    public string? SecondName { get; set; }

    /// <summary>
    /// Third name (الاسم الثالث)
    /// </summary>
    public string? ThirdName { get; set; }

    /// <summary>
    /// Family name (إسم الأسرة)
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// National ID or Passport number (Required)
    /// </summary>
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// Nationality (الجنسية) — lookup Country
    /// </summary>
    public int? NationalityCountryId { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Place of birth
    /// </summary>
    public string? PlaceOfBirth { get; set; }

    /// <summary>
    /// Foreign key to Education Level lookup
    /// </summary>
    public int? EducationLevelId { get; set; }

    /// <summary>
    /// Current job/occupation
    /// </summary>
    public string? Job { get; set; }

    /// <summary>
    /// Monthly income amount
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Foreign key to Health Status lookup
    /// </summary>
    public int? HealthStatusId { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Indicates if mother is alive (default: true)
    /// </summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>
    /// Indicates if mother is the family provider (default: false)
    /// </summary>
    public bool IsProvider { get; set; } = false;

    /// <summary>
    /// Date of death (if not alive)
    /// </summary>
    public DateTime? DeathDate { get; set; }

    /// <summary>
    /// Cause of death (سبب الوفاة) — lookup DeathReason (طبيعية / مرض / حادث)
    /// </summary>
    public int? DeathReasonId { get; set; }

    /// <summary>
    /// Death certificate image (صوره شهاده الوفاه) — framework attachment id
    /// </summary>
    public Guid? DeathCertificateAttachmentId { get; set; }

    /// <summary>
    /// Meza card number (رقم كارت ميزه)
    /// </summary>
    public string? MezaCard { get; set; }

    /// <summary>
    /// Meza card expiry date (تاريخ انتهاء الكارت)
    /// </summary>
    public DateTime? MezaCardExpirationDate { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
    public virtual EducationLevel? EducationLevel { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }
    public virtual Lookups.Country? Country { get; set; }
    public virtual Lookups.DeathReason? DeathReason { get; set; }
}
