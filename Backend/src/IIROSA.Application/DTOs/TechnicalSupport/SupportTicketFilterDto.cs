namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Support Ticket Filter DTO
/// Used for filtering and searching tickets (UC-13.9: Search Tickets)
/// </summary>
public class SupportTicketFilterDto
{
    // Search
    public string? SearchTerm { get; set; }

    // Filters
    public int? CategoryId { get; set; }
    public int? PriorityId { get; set; }
    public int? StatusId { get; set; }
    public bool? IsSolved { get; set; }
    public string? AssignedTo { get; set; }
    public string? CreatedByUserId { get; set; }

    // Date Range
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Sorting
    public string? SortBy { get; set; } = "CreatedOn";
    public string? SortDirection { get; set; } = "Descending";

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
