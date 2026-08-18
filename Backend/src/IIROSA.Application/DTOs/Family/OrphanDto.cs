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
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? SponsorshipStatus { get; set; }
    public string? CharityName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create Orphan DTO - Used for adding orphan to family (UC-4.4: Add Orphan to Family)
/// </summary>
public class CreateOrphanDto
{
    // Basic Information
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
}
