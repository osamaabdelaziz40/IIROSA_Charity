using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Create Seasonal Aid Campaign DTO - Used for creating new campaigns (UC-9.1)
/// </summary>
public class CreateSeasonalAidCampaignDto
{
    // Basic Information (UC-9.1)
    [Required(ErrorMessage = "Campaign name is required")]
    [StringLength(200, ErrorMessage = "Campaign name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campaign type is required")]
    [StringLength(50, ErrorMessage = "Campaign type cannot exceed 50 characters")]
    public string CampaignType { get; set; } = string.Empty; // Ramadan, Eid Al-Fitr, Eid Al-Adha, Winter, School Supplies, Other

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    // Campaign Period (UC-9.2)
    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    // Financial Information (UC-9.3)
    [Required(ErrorMessage = "Total budget is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total budget must be greater than zero")]
    public decimal TotalBudget { get; set; }

    [Required(ErrorMessage = "Budget currency is required")]
    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    public string BudgetCurrency { get; set; } = "EGP"; // EGP, SAR, USD

    [Required(ErrorMessage = "Per-family allocation is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Per-family allocation must be greater than zero")]
    public decimal PerFamilyAllocation { get; set; }

    // Geographic Scope
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }

    // Beneficiary Criteria (UC-9.1)
    [Range(1, int.MaxValue, ErrorMessage = "Maximum families must be at least 1")]
    public int? MaximumFamilies { get; set; }

    [StringLength(50, ErrorMessage = "Family type cannot exceed 50 characters")]
    public string? FamilyType { get; set; } // All, Orphan Families, Needy Families

    [Range(0, 18, ErrorMessage = "Minimum children age must be between 0 and 18")]
    public int? MinChildrenAge { get; set; }

    [Range(0, 18, ErrorMessage = "Maximum children age must be between 0 and 18")]
    public int? MaxChildrenAge { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
}
