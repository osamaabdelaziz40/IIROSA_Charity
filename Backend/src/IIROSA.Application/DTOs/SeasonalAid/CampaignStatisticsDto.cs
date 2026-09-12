namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Register statistics for the band above the campaigns grid (UC-9.6).
/// Counts follow the caller's country scope — the same pin the campaign list applies —
/// so the band describes the caller's whole register, not the current search.
/// </summary>
public class CampaignStatisticsDto
{
    public int Total { get; set; }

    /// <summary>Campaigns currently marked active (IsActive).</summary>
    public int Active { get; set; }

    /// <summary>Campaigns closed via UC-9.9 (IsClosed).</summary>
    public int Closed { get; set; }

    /// <summary>Campaigns created since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
