using IIROSA.Application.DTOs.TechnicalSupport;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Support Ticket Service Interface
/// Implements all use cases UC-13.1 through UC-13.10
/// </summary>
public interface ISupportTicketService
{
    // UC-13.1: Create Support Ticket
    Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId);

    // UC-13.2: Attach File to Ticket (handled as update)
    Task<SupportTicketDto> AttachFileToTicketAsync(Guid ticketId, string fileName, string filePath, long fileSize);

    // UC-CST-04: Update Support Ticket (Admin/Super Admin only)
    Task<SupportTicketDto> UpdateTicketAsync(UpdateSupportTicketDto dto, string userId);

    // Ticket form lookups (categories, priorities, statuses)
    Task<TicketLookupsDto> GetTicketLookupsAsync();

    // UC-13.3: View My Tickets
    Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetMyTicketsAsync(string userId, SupportTicketFilterDto filter);

    // UC-13.4: View All Tickets (Admin/Super Admin only)
    Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetAllTicketsAsync(SupportTicketFilterDto filter);

    // List export to Excel — allTickets selects the admin-wide read, otherwise the caller's own rows
    Task<byte[]> ExportTicketsToExcelAsync(SupportTicketFilterDto filter, string userId, bool allTickets);

    // UC-13.5: Update Ticket Status (Admin/Super Admin only)
    Task UpdateTicketStatusAsync(UpdateTicketStatusDto dto, string adminUserId);

    // UC-13.6: Mark Ticket as Solved (Admin/Super Admin only)
    Task MarkTicketAsSolvedAsync(MarkTicketSolvedDto dto, string adminUserId);

    // UC-13.7: Add Ticket Response (Admin/Super Admin only)
    Task<TicketResponseDto> AddTicketResponseAsync(CreateTicketResponseDto dto, string responderUserId, string responderName, string responderEmail);

    // UC-13.8: View Ticket Details (isAdmin bypasses the creator-only access check)
    Task<SupportTicketDetailDto> GetTicketDetailsAsync(Guid ticketId, string userId, bool isAdmin = false);

    // UC-13.9: Search Tickets (Admin/Super Admin only)
    Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> SearchTicketsAsync(SupportTicketFilterDto filter);

    // UC-13.10: Generate Support Report (Admin/Super Admin only)
    Task<SupportTicketReportDto> GenerateReportAsync(DateTime startDate, DateTime endDate);

    // Additional helper methods
    Task<SupportTicketDto?> GetByIdAsync(Guid id);
    Task<SupportTicketDto?> GetByCodeAsync(string code);
    Task<bool> HasUserAccessAsync(Guid ticketId, string userId);
    Task DeleteTicketAsync(Guid id, string deletedBy);

    // Assignment
    Task AssignTicketAsync(Guid ticketId, string assignedToUserId);

    // Statistics
    Task<int> GetMyTicketsCountAsync(string userId);
    Task<int> GetAllTicketsCountAsync();
    Task<int> GetUnsolvedTicketsCountAsync();
}
