namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Filter DTO - Used for filtering charities (UC-3.10)
/// </summary>
public class CharityFilterDto
{
    public string? SearchTerm { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsAddEnabled { get; set; }
    public bool? IsUpdateEnabled { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}
