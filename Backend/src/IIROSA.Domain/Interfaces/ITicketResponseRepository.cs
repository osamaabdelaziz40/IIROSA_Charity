using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.TechnicalSupport;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Ticket Response Repository Interface
/// Provides data access methods for TicketResponse entity
/// Implements use case UC-13.7: Add Ticket Response
/// </summary>
public interface ITicketResponseRepository : IRepository<TicketResponse>
{
    // Ticket-Specific Queries (UC-13.7, UC-13.8: View Ticket Details)
    Task<IEnumerable<TicketResponse>> GetByTicketIdAsync(Guid ticketId);
    Task<(IEnumerable<TicketResponse> Items, int TotalCount)> GetByTicketIdPagedAsync(Guid ticketId, int pageNumber, int pageSize);

    // Public vs Internal Notes (UC-13.7, UC-13.8)
    Task<IEnumerable<TicketResponse>> GetPublicResponsesByTicketIdAsync(Guid ticketId);
    Task<IEnumerable<TicketResponse>> GetInternalNotesByTicketIdAsync(Guid ticketId);
    Task<IEnumerable<TicketResponse>> GetAllResponsesByTicketIdAsync(Guid ticketId);  // For Admin

    // User-Specific Queries
    Task<IEnumerable<TicketResponse>> GetByResponderIdAsync(string responderUserId);

    // Statistics
    Task<int> GetResponseCountByTicketIdAsync(Guid ticketId);

    // Include Operations
    System.Linq.IQueryable<TicketResponse> IncludeAllNavigationProperties();
}
