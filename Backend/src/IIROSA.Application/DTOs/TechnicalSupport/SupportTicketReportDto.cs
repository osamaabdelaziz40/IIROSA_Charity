namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Support Ticket Report DTO
/// Used for UC-13.10: Generate Support Report
/// </summary>
public class SupportTicketReportDto
{
    // Summary Statistics
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int SolvedTickets { get; set; }
    public int UnsolvedTickets { get; set; }

    // Tickets by Status
    public Dictionary<string, int> TicketsByStatus { get; set; } = new();

    // Tickets by Priority
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();

    // Tickets by Category
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();

    // Performance Metrics
    public decimal AverageResolutionTimeHours { get; set; }
    public int TicketsResolvedWithinSLA { get; set; }
    public int TicketsBreachedSLA { get; set; }

    // User Statistics
    public Dictionary<string, int> TicketsByCreator { get; set; } = new();

    // Date Range
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Detailed Ticket List
    public IEnumerable<SupportTicketListDto> Tickets { get; set; } = new List<SupportTicketListDto>();
}
