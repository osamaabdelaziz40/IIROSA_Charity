using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.TechnicalSupport;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Support Ticket Repository Interface
/// Provides data access methods for SupportTicket entity
/// Implements all use cases UC-13.1 through UC-13.10
/// </summary>
public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    // User-Specific Queries (UC-13.3: View My Tickets)
    Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId);
    Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetByUserIdPagedAsync(string userId, int pageNumber, int pageSize);

    // Admin Queries (UC-13.4: View All Tickets)
    // searchTerm/sortBy/sortDirection are honoured by the implementation — the old
    // 8-param shape silently dropped the list's search box and sortable headers.
    Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllTicketsPagedAsync(
        int pageNumber, int pageSize,
        int? categoryId = null, int? priorityId = null, int? statusId = null,
        string? createdByUserId = null, string? assignedTo = null,
        DateTime? startDate = null, DateTime? endDate = null,
        bool? isSolved = null, string? searchTerm = null,
        string? sortBy = null, string? sortDirection = null);

    // Search and Filter (UC-13.9: Search Tickets)
    Task<IEnumerable<SupportTicket>> SearchAsync(string searchTerm);
    Task<IEnumerable<SupportTicket>> GetFilteredAsync(
        int? categoryId = null, int? priorityId = null, int? statusId = null,
        string? assignedTo = null, DateTime? startDate = null, DateTime? endDate = null);

    // Category, Priority, Status Filters (UC-13.4, UC-13.9)
    Task<IEnumerable<SupportTicket>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<SupportTicket>> GetByPriorityAsync(int priorityId);
    Task<IEnumerable<SupportTicket>> GetByStatusAsync(int statusId);
    Task<IEnumerable<SupportTicket>> GetByAssignedToAsync(string assignedToUserId);

    // Resolution Queries (UC-13.6: Mark Ticket as Solved)
    Task<IEnumerable<SupportTicket>> GetSolvedTicketsAsync();
    Task<IEnumerable<SupportTicket>> GetUnsolvedTicketsAsync();
    Task<IEnumerable<SupportTicket>> GetTicketsByResolvedByAsync(string resolvedByUserId);

    // Statistics (UC-13.10: Generate Support Report)
    Task<int> GetTotalTicketsCountAsync();
    Task<int> GetTicketsByStatusCountAsync(int statusId);
    Task<int> GetTicketsByCategoryCountAsync(int categoryId);
    Task<int> GetTicketsByPriorityCountAsync(int priorityId);
    Task<int> GetSolvedTicketsCountAsync();
    Task<int> GetUnsolvedTicketsCountAsync();
    Task<decimal> GetAverageResolutionTimeAsync();

    // Date Range Queries (UC-13.10: Generate Support Report)
    Task<IEnumerable<SupportTicket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCreatedCountInDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetResolvedCountInDateRangeAsync(DateTime startDate, DateTime endDate);

    // Specific Queries
    Task<bool> ExistsAsync(Guid id);
    Task<bool> HasUserAccessAsync(Guid ticketId, string userId);

    // Include Operations
    System.Linq.IQueryable<SupportTicket> IncludeAllNavigationProperties();
}
