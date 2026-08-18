namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Housing Project Filter DTO - Used for filtering and searching projects (UC-10.6)
/// </summary>
public class HousingProjectFilterDto
{
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Search
    public string? SearchTerm { get; set; }

    // Filters
    public string? ProjectType { get; set; }
    public string? ProjectStatus { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public Guid? CharityId { get; set; }
    public string? HousingType { get; set; }

    // Date Range Filters
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? ExpectedEndDateFrom { get; set; }
    public DateTime? ExpectedEndDateTo { get; set; }

    // Completion Filters
    public int? MinCompletionPercentage { get; set; }
    public int? MaxCompletionPercentage { get; set; }

    // Sorting
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
