using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Provider DTO - Non-parent provider information
/// </summary>
public class ProviderDto
{
    public Guid Id { get; set; }
    public Guid? FamilyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RelationshipToFamily { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Job { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Create Provider DTO - Used for adding non-parent provider (UC-4.6: Add Non-Parent Provider)
/// </summary>
public class CreateProviderDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Relationship to family is required")]
    [StringLength(100, ErrorMessage = "Relationship cannot exceed 100 characters")]
    public string RelationshipToFamily { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required")]
    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Update Provider DTO
/// </summary>
public class UpdateProviderDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    [StringLength(100, ErrorMessage = "Relationship cannot exceed 100 characters")]
    public string? RelationshipToFamily { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}
