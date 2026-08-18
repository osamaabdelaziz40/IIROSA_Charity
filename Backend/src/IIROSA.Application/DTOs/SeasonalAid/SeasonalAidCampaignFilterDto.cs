namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Campaign Filter DTO - Used for filtering campaigns (UC-9.6)
/// </summary>
public class SeasonalAidCampaignFilterDto
{
    // Search
    public string? SearchTerm { get; set; }

    // Filters
    public string? CampaignType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsClosed { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public Guid? CharityId { get; set; }

    // Date Range
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}
