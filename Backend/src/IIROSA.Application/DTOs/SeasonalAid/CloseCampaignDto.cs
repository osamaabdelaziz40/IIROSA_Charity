using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Close Campaign DTO - Used for closing campaigns (UC-9.9)
/// </summary>
public class CloseCampaignDto
{
    [Required(ErrorMessage = "Campaign ID is required")]
    public Guid CampaignId { get; set; }

    [StringLength(2000, ErrorMessage = "Closure notes cannot exceed 2000 characters")]
    public string? ClosureNotes { get; set; }
}
