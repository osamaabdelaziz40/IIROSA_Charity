using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Orphan entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements use cases UC-4.4 (Add Orphan to Family) and UC-4.11 (Update Orphan Details)
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// </summary>
public class Orphan : FullAuditedEntity
{
    // Basic Information
    /// <summary>
    /// Orphan code (auto-generated)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the orphan (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Date of birth (Required)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Place of birth
    /// </summary>
    public string? PlaceOfBirth { get; set; }

    /// <summary>
    /// Gender: Male, Female
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// National ID or Passport number
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// Photo file path/attachment ID
    /// </summary>
    public Guid? PhotoAttachmentId { get; set; }

    /// <summary>
    /// Foreign key to Charity (data isolation)
    /// </summary>
    public Guid? FK_CharityId { get; set; }

    // Orphan Status
    /// <summary>
    /// Orphan type: Father-deceased, Mother-deceased, Both-deceased
    /// </summary>
    public string? OrphanType { get; set; }

    /// <summary>
    /// Sponsorship status: Sponsored, Unsponsored, Pending
    /// </summary>
    public string? SponsorshipStatus { get; set; }

    /// <summary>
    /// Sponsorship start date
    /// </summary>
    public DateTime? SponsorshipStartDate { get; set; }

    /// <summary>
    /// Foreign key to Sponsor (if sponsored)
    /// </summary>
    public Guid? SponsorId { get; set; }

    /// <summary>
    /// Monthly sponsorship amount
    /// </summary>
    public decimal? MonthlyAmount { get; set; }

    // Education
    /// <summary>
    /// Foreign key to Education Level lookup
    /// </summary>
    public int? EducationLevelId { get; set; }

    /// <summary>
    /// School name
    /// </summary>
    public string? SchoolName { get; set; }

    /// <summary>
    /// Grade or class
    /// </summary>
    public string? GradeClass { get; set; }

    /// <summary>
    /// Academic performance/notes
    /// </summary>
    public string? AcademicPerformance { get; set; }

    // Health
    /// <summary>
    /// Foreign key to Health Status lookup
    /// </summary>
    public int? HealthStatusId { get; set; }

    /// <summary>
    /// Disabilities (if any)
    /// </summary>
    public string? Disabilities { get; set; }

    /// <summary>
    /// Chronic diseases
    /// </summary>
    public string? ChronicDiseases { get; set; }

    // Contact
    /// <summary>
    /// Phone number (if has own)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    // Other
    /// <summary>
    /// Hobbies
    /// </summary>
    public string? Hobbies { get; set; }

    /// <summary>
    /// Skills
    /// </summary>
    public string? Skills { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
    public virtual Sponsor? Sponsor { get; set; }
    public virtual EducationLevel? EducationLevel { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }
    public virtual ICollection<PeriodicOrphanReport> PeriodicReports { get; set; } = new List<PeriodicOrphanReport>();
}
