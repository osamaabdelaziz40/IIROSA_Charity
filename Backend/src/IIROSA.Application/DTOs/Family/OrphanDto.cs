using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Orphan DTO - Detailed orphan information (UC-4.13: View Family Details)
/// </summary>
public class OrphanDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? NationalId { get; set; }
    public Guid? PhotoAttachmentId { get; set; }
    /// <summary>§11.S.2 اضافة ابن صوره شهاده الميلاد — UC-HOU-04 read round-trip.</summary>
    public Guid? BirthCertificateAttachmentId { get; set; }
    /// <summary>§11.S.2 اضافة ابن صوره إثبات القيد — UC-HOU-04 read round-trip.</summary>
    public Guid? EnrollmentAttachmentId { get; set; }
    public Guid? FK_CharityId { get; set; }
    public string? OrphanType { get; set; }
    public string? SponsorshipStatus { get; set; }
    public DateTime? SponsorshipStartDate { get; set; }
    public Guid? SponsorId { get; set; }
    public string? SponsorName { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public int? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeClass { get; set; }
    public string? AcademicPerformance { get; set; }
    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
    public string? Disabilities { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Hobbies { get; set; }
    public string? Skills { get; set; }
    public string? Notes { get; set; }
    // Refugee register extension (epic 7, UC-REF-03 §12.S.2 اضافة ابن)
    public int? SocialStatusId { get; set; }
    public string? SocialStatusName { get; set; }
    // Housing register extensions (epic 6, §11.S.2 اضافة ابن) — read round-trip for UC-HOU-04
    public string? Profession { get; set; }
    public string? DepartmentName { get; set; }
    public string? FacultyName { get; set; }
    /// <summary>§11.S.2 «حاصل على مؤهل دراسى» — mandatory for new housing children
    /// (review D3 2026-08-24); nullable so legacy rows round-trip.</summary>
    public int? EducationalQualificationId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// Orphan List DTO - Summary view for orphan list
/// </summary>
public class OrphanListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    /// <summary>National ID — shown on the family members screen (UC-4.4/§10.S.2 add-orphan modal
    /// reads it back when re-opening an orphan).</summary>
    public string? NationalId { get; set; }
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? SponsorshipStatus { get; set; }
    public string? CharityName { get; set; }
    // Refugee register extension (epic 7, UC-REF-03 §12.S.2 اضافة ابن)
    public int? SocialStatusId { get; set; }
    public string? SocialStatusName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create Orphan DTO - Used for adding orphan to family (UC-4.4: Add Orphan to Family)
/// </summary>
public class CreateOrphanDto
{
    // Basic Information
    /// <summary>
    /// UC-HOU-04 edit-sync key: on the housing aggregate update, a payload child carrying an
    /// id updates that orphan; one without is added. Always null on the create path.
    /// </summary>
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
    public string? Gender { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    public Guid? PhotoAttachmentId { get; set; }

    // Orphan Status
    [StringLength(50, ErrorMessage = "Orphan type cannot exceed 50 characters")]
    public string? OrphanType { get; set; }

    [StringLength(50, ErrorMessage = "Sponsorship status cannot exceed 50 characters")]
    public string? SponsorshipStatus { get; set; }

    public DateTime? SponsorshipStartDate { get; set; }

    // Education
    public int? EducationLevelId { get; set; }

    [StringLength(200, ErrorMessage = "School name cannot exceed 200 characters")]
    public string? SchoolName { get; set; }

    [StringLength(50, ErrorMessage = "Grade/Class cannot exceed 50 characters")]
    public string? GradeClass { get; set; }

    [StringLength(500, ErrorMessage = "Academic performance cannot exceed 500 characters")]
    public string? AcademicPerformance { get; set; }

    // Health
    public int? HealthStatusId { get; set; }

    [StringLength(500, ErrorMessage = "Disabilities cannot exceed 500 characters")]
    public string? Disabilities { get; set; }

    [StringLength(500, ErrorMessage = "Chronic diseases cannot exceed 500 characters")]
    public string? ChronicDiseases { get; set; }

    // Contact
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    // Other
    [StringLength(500, ErrorMessage = "Hobbies cannot exceed 500 characters")]
    public string? Hobbies { get; set; }

    [StringLength(500, ErrorMessage = "Skills cannot exceed 500 characters")]
    public string? Skills { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Refugee register extension (epic 7, UC-REF-03 §12.S.2 اضافة ابن)
    public int? SocialStatusId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة ابن)
    [StringLength(100, ErrorMessage = "Profession cannot exceed 100 characters")]
    public string? Profession { get; set; }
    [StringLength(200, ErrorMessage = "Department cannot exceed 200 characters")]
    public string? DepartmentName { get; set; }
    [StringLength(200, ErrorMessage = "Faculty cannot exceed 200 characters")]
    public string? FacultyName { get; set; }
    public Guid? BirthCertificateAttachmentId { get; set; }
    public Guid? EnrollmentAttachmentId { get; set; }
    /// <summary>§11.S.2 «حاصل على مؤهل دراسى» (review D3 2026-08-24) — EducationLevel
    /// catalogue id; the housing validator requires it for NEW children.</summary>
    public int? EducationalQualificationId { get; set; }
}

/// <summary>
/// Update Orphan DTO - Used for updating orphan information (UC-4.11: Update Orphan Details)
/// </summary>
public class UpdateOrphanDto
{
    [Required]
    public Guid Id { get; set; }

    // Basic Information
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
    public string? Gender { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    public Guid? PhotoAttachmentId { get; set; }

    // Orphan Status
    [StringLength(50, ErrorMessage = "Orphan type cannot exceed 50 characters")]
    public string? OrphanType { get; set; }

    [StringLength(50, ErrorMessage = "Sponsorship status cannot exceed 50 characters")]
    public string? SponsorshipStatus { get; set; }

    public DateTime? SponsorshipStartDate { get; set; }

    // Education
    public int? EducationLevelId { get; set; }

    [StringLength(200, ErrorMessage = "School name cannot exceed 200 characters")]
    public string? SchoolName { get; set; }

    [StringLength(50, ErrorMessage = "Grade/Class cannot exceed 50 characters")]
    public string? GradeClass { get; set; }

    [StringLength(500, ErrorMessage = "Academic performance cannot exceed 500 characters")]
    public string? AcademicPerformance { get; set; }

    // Health
    public int? HealthStatusId { get; set; }

    [StringLength(500, ErrorMessage = "Disabilities cannot exceed 500 characters")]
    public string? Disabilities { get; set; }

    [StringLength(500, ErrorMessage = "Chronic diseases cannot exceed 500 characters")]
    public string? ChronicDiseases { get; set; }

    // Contact
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    // Other
    [StringLength(500, ErrorMessage = "Hobbies cannot exceed 500 characters")]
    public string? Hobbies { get; set; }

    [StringLength(500, ErrorMessage = "Skills cannot exceed 500 characters")]
    public string? Skills { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Refugee register extension (epic 7, UC-REF-03 §12.S.2 اضافة ابن)
    public int? SocialStatusId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة ابن)
    [StringLength(100, ErrorMessage = "Profession cannot exceed 100 characters")]
    public string? Profession { get; set; }
    [StringLength(200, ErrorMessage = "Department cannot exceed 200 characters")]
    public string? DepartmentName { get; set; }
    [StringLength(200, ErrorMessage = "Faculty cannot exceed 200 characters")]
    public string? FacultyName { get; set; }
    public Guid? BirthCertificateAttachmentId { get; set; }
    public Guid? EnrollmentAttachmentId { get; set; }
}
