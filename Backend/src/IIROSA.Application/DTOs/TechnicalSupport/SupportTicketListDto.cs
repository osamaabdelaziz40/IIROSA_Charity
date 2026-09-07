namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Support Ticket List DTO
/// Used for displaying tickets in a list/grid view (UC-13.3, UC-13.4)
/// </summary>
public class SupportTicketListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Lookup Properties
    // StatusId — the list row actions gate solve/close on it; previously only StatusName
    // was returned, so `item.statusId !== closedStatusId` was always true and the actions
    // showed on already-closed rows.
    public int StatusId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;

    // Resolution
    public bool IsSolved { get; set; }

    // Assignment
    public string? AssignedToName { get; set; }

    // User Information
    public string CreatedByUserName { get; set; } = string.Empty;

    // Dates
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    // Response Count
    public int ResponseCount { get; set; }

    // Priority Color (for UI)
    public string? PriorityColor { get; set; }

    // Status Color (for UI)
    public string? StatusColor { get; set; }
}
