using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Mother DTO - Mother information
/// </summary>
public class MotherDto
{
    public Guid Id { get; set; }
    public Guid? FamilyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public int? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public string? Job { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
    public string? Phone { get; set; }
    public bool IsAlive { get; set; }
    public bool IsProvider { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Create Mother DTO - Used for adding mother to family (UC-4.3: Add Family Mother)
/// </summary>
public class CreateMotherDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required")]
    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    public int? EducationLevelId { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public int? HealthStatusId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsProvider { get; set; } = false;

    public DateTime? DeathDate { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Update Mother DTO - Used for updating mother information (UC-4.10: Update Mother Details)
/// </summary>
public class UpdateMotherDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Place of birth cannot exceed 200 characters")]
    public string? PlaceOfBirth { get; set; }

    public int? EducationLevelId { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public int? HealthStatusId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    public bool? IsAlive { get; set; }

    public bool? IsProvider { get; set; }

    public DateTime? DeathDate { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}
