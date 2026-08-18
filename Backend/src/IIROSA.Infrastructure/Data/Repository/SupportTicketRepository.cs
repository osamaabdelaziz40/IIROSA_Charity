using IIROSA.Application.DTOs.TechnicalSupport;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.TechnicalSupport;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Support Ticket Repository Implementation
/// Implements all data access methods for SupportTicket entity
/// </summary>
public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
{
    private readonly DbSet<SupportTicket> _dbSet;

    public SupportTicketRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<SupportTicket>();
    }

    // User-Specific Queries (UC-13.3: View My Tickets)
    public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetByUserIdPagedAsync(string userId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedOn);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Admin Queries (UC-13.4: View All Tickets)
    public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllTicketsPagedAsync(SupportTicketFilterDto filter)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(t =>
                t.Title.Contains(filter.SearchTerm) ||
                t.Message.Contains(filter.SearchTerm));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.PriorityId.HasValue)
        {
            query = query.Where(t => t.PriorityId == filter.PriorityId.Value);
        }

        if (filter.StatusId.HasValue)
        {
            query = query.Where(t => t.StatusId == filter.StatusId.Value);
        }

        if (filter.IsSolved.HasValue)
        {
            query = query.Where(t => t.IsSolved == filter.IsSolved.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
        {
            query = query.Where(t => t.AssignedTo == filter.AssignedTo);
        }

        if (!string.IsNullOrWhiteSpace(filter.CreatedByUserId))
        {
            query = query.Where(t => t.CreatedByUserId == filter.CreatedByUserId);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedOn >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedOn <= filter.EndDate.Value);
        }

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Search and Filter (UC-13.9: Search Tickets)
    public async Task<IEnumerable<SupportTicket>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t =>
                t.Title.Contains(searchTerm) ||
                t.Message.Contains(searchTerm))
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetFilteredAsync(SupportTicketFilterDto filter)
    {
        var (items, _) = await GetAllTicketsPagedAsync(filter);
        return items;
    }

    // Category, Priority, Status Filters
    public async Task<IEnumerable<SupportTicket>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByPriorityAsync(int priorityId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.PriorityId == priorityId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByStatusAsync(int statusId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.StatusId == statusId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByAssignedToAsync(string assignedToUserId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.AssignedTo == assignedToUserId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    // Resolution Queries (UC-13.6: Mark Ticket as Solved)
    public async Task<IEnumerable<SupportTicket>> GetSolvedTicketsAsync()
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.IsSolved)
            .OrderByDescending(t => t.ResolvedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetUnsolvedTicketsAsync()
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => !t.IsSolved)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetTicketsByResolvedByAsync(string resolvedByUserId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.ResolvedBy == resolvedByUserId)
            .OrderByDescending(t => t.ResolvedOn)
            .ToListAsync();
    }

    // Statistics (UC-13.10: Generate Support Report)
    public async Task<int> GetTotalTicketsCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> GetTicketsByStatusCountAsync(int statusId)
    {
        return await _dbSet.CountAsync(t => t.StatusId == statusId);
    }

    public async Task<int> GetTicketsByCategoryCountAsync(int categoryId)
    {
        return await _dbSet.CountAsync(t => t.CategoryId == categoryId);
    }

    public async Task<int> GetTicketsByPriorityCountAsync(int priorityId)
    {
        return await _dbSet.CountAsync(t => t.PriorityId == priorityId);
    }

    public async Task<int> GetSolvedTicketsCountAsync()
    {
        return await _dbSet.CountAsync(t => t.IsSolved);
    }

    public async Task<int> GetUnsolvedTicketsCountAsync()
    {
        return await _dbSet.CountAsync(t => !t.IsSolved);
    }

    public async Task<decimal> GetAverageResolutionTimeAsync()
    {
        var solvedTickets = await _dbSet
            .Where(t => t.IsSolved && t.ResolvedOn.HasValue)
            .ToListAsync();

        if (!solvedTickets.Any())
            return 0;

        var totalHours = solvedTickets
            .Sum(t => (t.ResolvedOn!.Value - t.CreatedOn).TotalHours);

        return (decimal)(totalHours / solvedTickets.Count);
    }

    // Date Range Queries (UC-13.10: Generate Support Report)
    public async Task<IEnumerable<SupportTicket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Where(t => t.CreatedOn >= startDate && t.CreatedOn <= endDate)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<int> GetCreatedCountInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .CountAsync(t => t.CreatedOn >= startDate && t.CreatedOn <= endDate);
    }

    public async Task<int> GetResolvedCountInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .CountAsync(t => t.ResolvedOn >= startDate && t.ResolvedOn <= endDate);
    }

    // Specific Queries
    public async Task<bool> HasUserAccessAsync(Guid ticketId, string userId)
    {
        var ticket = await _dbSet.FindAsync(ticketId);
        if (ticket == null)
            return false;

        // User has access if they created the ticket
        return ticket.CreatedByUserId == userId;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(t => t.Id == id);
    }

    // Include Operations
    public System.Linq.IQueryable<SupportTicket> IncludeAllNavigationProperties()
    {
        return _dbSet
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Responses);
    }

    // Private helper for sorting
    private IQueryable<SupportTicket> ApplySorting(IQueryable<SupportTicket> query, string? sortBy, string? sortDirection)
    {
        return sortBy?.ToLower() switch
        {
            "title" => sortDirection?.ToLower() == "ascending"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
            "createdon" => sortDirection?.ToLower() == "ascending"
                ? query.OrderBy(t => t.CreatedOn)
                : query.OrderByDescending(t => t.CreatedOn),
            "updatedon" => sortDirection?.ToLower() == "ascending"
                ? query.OrderBy(t => t.UpdatedOn)
                : query.OrderByDescending(t => t.UpdatedOn),
            "priority" => sortDirection?.ToLower() == "ascending"
                ? query.OrderBy(t => t.PriorityId)
                : query.OrderByDescending(t => t.PriorityId),
            "status" => sortDirection?.ToLower() == "ascending"
                ? query.OrderBy(t => t.StatusId)
                : query.OrderByDescending(t => t.StatusId),
            _ => query.OrderByDescending(t => t.CreatedOn)
        };
    }

    // Interface-compliant methods
    public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllTicketsPagedAsync(
        int page, int pageSize, int? categoryId = null, int? priorityId = null, int? statusId = null,
        string? createdByUserId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = IncludeAllNavigationProperties();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (priorityId.HasValue)
            query = query.Where(t => t.PriorityId == priorityId.Value);

        if (statusId.HasValue)
            query = query.Where(t => t.StatusId == statusId.Value);

        if (!string.IsNullOrWhiteSpace(createdByUserId))
            query = query.Where(t => t.CreatedByUserId == createdByUserId);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedOn >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedOn <= endDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<SupportTicket>> GetFilteredAsync(
        int? categoryId = null, int? priorityId = null, int? statusId = null,
        string? assignedTo = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = IncludeAllNavigationProperties();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (priorityId.HasValue)
            query = query.Where(t => t.PriorityId == priorityId.Value);

        if (statusId.HasValue)
            query = query.Where(t => t.StatusId == statusId.Value);

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(t => t.AssignedTo == assignedTo);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedOn >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedOn <= endDate.Value);

        return await query.OrderByDescending(t => t.CreatedOn).ToListAsync();
    }
}
