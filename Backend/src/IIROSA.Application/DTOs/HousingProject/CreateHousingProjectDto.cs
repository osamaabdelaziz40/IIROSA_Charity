using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Create Housing Project DTO - Used for creating new housing projects (UC-10.1)
/// </summary>
public class CreateHousingProjectDto
{
    // Basic Information (UC-10.1)
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project type is required")]
    [StringLength(50, ErrorMessage = "Project type cannot exceed 50 characters")]
    public string ProjectType { get; set; } = string.Empty; // New Construction, Renovation, Repair, Expansion

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Expected end date is required")]
    public DateTime ExpectedEndDate { get; set; }

    // Location (UC-10.1)
    [Required(ErrorMessage = "Country is required")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Region is required")]
    public int RegionId { get; set; }

    [Required(ErrorMessage = "Center is required")]
    public int CenterId { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Village is required")]
    [StringLength(200, ErrorMessage = "Village cannot exceed 200 characters")]
    public string Village { get; set; } = string.Empty;

    [Required(ErrorMessage = "GPS coordinates are required")]
    [StringLength(100, ErrorMessage = "GPS coordinates cannot exceed 100 characters")]
    public string GPSCoordinates { get; set; } = string.Empty;

    // Specifications (UC-10.1)
    [Required(ErrorMessage = "Housing type is required")]
    [StringLength(50, ErrorMessage = "Housing type cannot exceed 50 characters")]
    public string HousingType { get; set; } = string.Empty; // Apartment, Villa, House, Room

    [Required(ErrorMessage = "Number of units is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of units must be at least 1")]
    public int NumberOfUnits { get; set; }

    [Required(ErrorMessage = "Area per unit is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Area per unit must be greater than zero")]
    public decimal AreaPerUnit { get; set; }

    [Required(ErrorMessage = "Total area is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total area must be greater than zero")]
    public decimal TotalArea { get; set; }

    // Financial Information (UC-10.2)
    [Required(ErrorMessage = "Total budget is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total budget must be greater than zero")]
    public decimal TotalBudget { get; set; }

    [Required(ErrorMessage = "Budget currency is required")]
    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    public string BudgetCurrency { get; set; } = "EGP"; // EGP, SAR

    [Required(ErrorMessage = "Donor name is required")]
    [StringLength(200, ErrorMessage = "Donor name cannot exceed 200 characters")]
    public string DonorName { get; set; } = string.Empty;

    // Beneficiary Assignment (UC-10.3, UC-10.10)
    [Required(ErrorMessage = "Charity is required")]
    public Guid CharityId { get; set; }

    [Required(ErrorMessage = "Family is required")]
    public Guid FamilyId { get; set; }

    // Status
    [Required(ErrorMessage = "Project status is required")]
    [StringLength(50, ErrorMessage = "Project status cannot exceed 50 characters")]
    public string ProjectStatus { get; set; } = "Planning"; // Planning, In Progress, Completed, On Hold

    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public int CompletionPercentage { get; set; } = 0;
}
