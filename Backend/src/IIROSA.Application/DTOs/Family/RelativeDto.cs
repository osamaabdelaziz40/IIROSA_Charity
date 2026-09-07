using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Relative DTO - Relative information
/// </summary>
public class RelativeDto
{
    public Guid Id { get; set; }
    public Guid? FamilyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? NationalId { get; set; }
    public int? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public string? Job { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLivingWithFamily { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? Notes { get; set; }
    public int? Age { get; set; }

    // Audit fields
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create Relative DTO - Used for adding relative to family (UC-4.7: Add Family Relative)
/// </summary>
public class CreateRelativeDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Relationship type is required")]
    [StringLength(50, ErrorMessage = "Relationship type cannot exceed 50 characters")]
    public string RelationshipType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    public int? EducationLevelId { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public int? HealthStatusId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsLivingWithFamily { get; set; } = false;

    public DateTime? DeathDate { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Update Relative DTO - Used for updating relative information (UC-4.8: Update Relative Details)
/// </summary>
public class UpdateRelativeDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    [StringLength(50, ErrorMessage = "Relationship type cannot exceed 50 characters")]
    public string? RelationshipType { get; set; }

    [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    public int? EducationLevelId { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public int? HealthStatusId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    public bool? IsAlive { get; set; }

    public bool? IsLivingWithFamily { get; set; }

    public DateTime? DeathDate { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Relative List DTO - Simplified relative information for lists
/// </summary>
public class RelativeListDto
{
    public Guid Id { get; set; }
    public Guid? FamilyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int? Age { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLivingWithFamily { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }

    // Refugee register extensions (epic 7, UC-REF-04) — the §12.S.2 edit screen reloads its
    // staged مرافق rows from this shape; without the NID and the free-text صلة القرابة (stored
    // as notes) the row would silently lose them on reload. (Companion social status is NOT
    // here: Relative has no such column — see the 7-4 completion-note gap.)
    public string? NationalId { get; set; }
    public string? Notes { get; set; }

    // الحالة الصحية (epic-7 review P12) — §12.S.2 مرافق health status, included on the list
    // shape so the view/edit screens reload and display it like the orphan rows do
    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
}
