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

    /// <summary>
    /// §11.S.2 child mandatory field «حاصل على مؤهل دراسى» — the educational qualification
    /// the child holds (review decision D3, 2026-08-24: reuse the EducationLevel catalogue;
    /// no new lookup — 19-6 owns the reasons catalogue). Nullable column so legacy rows
    /// survive; the housing create validator requires it for NEW children.
    /// </summary>
    public int? EducationalQualificationId { get; set; }

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

    // Refugee register extension (epic 7, UC-REF-03 §12.S.2 اضافة ابن)

    /// <summary>
    /// Social status (الحالة الاجتماعية)
    /// </summary>
    public int? SocialStatusId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة ابن) — all nullable

    /// <summary>
    /// Occupation (نوعية العمل) — free text; no Profession lookup entity exists on this stack
    /// (deviation recorded in story 6-3, same deferral class as epic 8's prayer/hobby lookups)
    /// </summary>
    public string? Profession { get; set; }

    /// <summary>
    /// Department inside the educational institution (القسم)
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Faculty (الكلية)
    /// </summary>
    public string? FacultyName { get; set; }

    /// <summary>
    /// Birth certificate attachment (شهاده الميلاد)
    /// </summary>
    public Guid? BirthCertificateAttachmentId { get; set; }

    /// <summary>
    /// School enrolment proof attachment (القيد الدراسي)
    /// </summary>
    public Guid? EnrollmentAttachmentId { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
    public virtual Sponsor? Sponsor { get; set; }
    public virtual EducationLevel? EducationLevel { get; set; }
    /// <summary>«حاصل على مؤهل دراسى» — second EducationLevel edge, configured explicitly in
    /// OrphanConfiguration so it cannot collide with the EducationLevel nav above.</summary>
    public virtual EducationLevel? EducationalQualification { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }
    public virtual SocialStatus? SocialStatus { get; set; }
    public virtual ICollection<PeriodicOrphanReport> PeriodicReports { get; set; } = new List<PeriodicOrphanReport>();
}
