using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Set Family Received Flag DTO - UC-PRJ-08 (تأكيد استلام الأسرة).
/// Marks the project-family registration of family <c>{id}</c> (route) in campaign
/// <see cref="CampaignId"/> as delivered, or clears the flag. The acting user always comes
/// from the token, never from the payload.
/// </summary>
public class SetFamilyReceivedFlagDto
{
    [Required(ErrorMessage = "Project id is required")]
    public Guid CampaignId { get; set; }

    public bool IsReceived { get; set; }
}
