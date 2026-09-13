using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Create Seasonal Aid Beneficiary DTO - Used for registering beneficiaries (UC-9.4)
/// </summary>
public class CreateSeasonalAidBeneficiaryDto
{
    [Required(ErrorMessage = "Campaign ID is required")]
    public Guid CampaignId { get; set; }

    [Required(ErrorMessage = "At least one family must be selected")]
    public List<Guid> FamilyIds { get; set; } = new();

    [Range(0.01, double.MaxValue, ErrorMessage = "Allocation amount must be greater than zero")]
    public decimal? AllocationAmount { get; set; } // Optional - if not provided, uses campaign's default allocation

    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    public string? Currency { get; set; } // Optional - if not provided, uses campaign's default currency

    [StringLength(500, ErrorMessage = "Registration notes cannot exceed 500 characters")]
    public string? RegistrationNotes { get; set; }

    /// <summary>
    /// true (default) registers the families as main beneficiaries; false puts them on the
    /// pending list awaiting confirmation as main — the UC-PRJ-06 selection screen's two lists.
    /// </summary>
    public bool IsMain { get; set; } = true;
}
