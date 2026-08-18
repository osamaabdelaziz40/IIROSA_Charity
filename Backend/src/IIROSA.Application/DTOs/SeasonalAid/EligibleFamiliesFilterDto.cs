using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Eligible Families Filter DTO - Used for finding eligible families for a campaign (UC-9.4)
/// </summary>
public class EligibleFamiliesFilterDto
{
    [Required(ErrorMessage = "Campaign ID is required")]
    public Guid CampaignId { get; set; }

    // Filters
    public Guid? CharityId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? FamilyType { get; set; } // Orphan Families, Needy Families, All

    // Age Range for children
    public int? MinChildrenAge { get; set; }
    public int? MaxChildrenAge { get; set; }

    // Search
    public string? SearchTerm { get; set; } // Search by family code or address

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; } = "FamilyCode";
    public bool SortDescending { get; set; } = false;
}
