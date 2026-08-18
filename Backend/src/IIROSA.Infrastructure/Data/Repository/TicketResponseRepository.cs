using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.TechnicalSupport;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Ticket Response Repository Implementation
/// Implements all data access methods for TicketResponse entity
/// </summary>
public class TicketResponseRepository : Repository<TicketResponse>, ITicketResponseRepository
{
    private readonly DbSet<TicketResponse> _dbSet;

    public TicketResponseRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<TicketResponse>();
    }

    // Ticket-Specific Queries (UC-13.7, UC-13.8: View Ticket Details)
    public async Task<IEnumerable<TicketResponse>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _dbSet
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<(IEnumerable<TicketResponse> Items, int TotalCount)> GetByTicketIdPagedAsync(Guid ticketId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedOn);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Public vs Internal Notes (UC-13.7, UC-13.8)
    public async Task<IEnumerable<TicketResponse>> GetPublicResponsesByTicketIdAsync(Guid ticketId)
    {
        return await _dbSet
            .Where(r => r.TicketId == ticketId && !r.IsInternalNote)
            .OrderBy(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketResponse>> GetInternalNotesByTicketIdAsync(Guid ticketId)
    {
        return await _dbSet
            .Where(r => r.TicketId == ticketId && r.IsInternalNote)
            .OrderBy(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketResponse>> GetAllResponsesByTicketIdAsync(Guid ticketId)
    {
        return await GetByTicketIdAsync(ticketId);
    }

    // User-Specific Queries
    public async Task<IEnumerable<TicketResponse>> GetByResponderIdAsync(string responderUserId)
    {
        return await _dbSet
            .Include(r => r.Ticket)
            .Where(r => r.RespondedByUserId == responderUserId)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();
    }

    // Statistics
    public async Task<int> GetResponseCountByTicketIdAsync(Guid ticketId)
    {
        return await _dbSet
            .CountAsync(r => r.TicketId == ticketId);
    }

    // Include Operations
    public System.Linq.IQueryable<TicketResponse> IncludeAllNavigationProperties()
    {
        return _dbSet.Include(r => r.Ticket);
    }
}
