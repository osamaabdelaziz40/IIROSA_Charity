namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Filter DTO - Used for filtering charities (UC-3.10)
/// </summary>
public class CharityFilterDto
{
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Narrows the result to one charity. A head-office caller may set this to look at a single
    /// charity; for a charity-bound caller the service overwrites it with their own charity, so
    /// supplying someone else's id has no effect.
    /// </summary>
    public Guid? CharityId { get; set; }

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
