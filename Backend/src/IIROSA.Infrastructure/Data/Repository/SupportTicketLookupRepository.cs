using IIROSA.Domain.Entities.TechnicalSupport.Lookups;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Support Ticket Lookup Repository Implementation
/// Serves the three seeded lookup tables; one repository because the ticket form
/// consumes all three lists together.
/// </summary>
public class SupportTicketLookupRepository : ISupportTicketLookupRepository
{
    private readonly ApplicationDbContext _context;

    public SupportTicketLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync()
    {
        return await _context.Set<SupportTicketCategory>()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicketPriority>> GetPrioritiesAsync()
    {
        return await _context.Set<SupportTicketPriority>()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SeverityLevel)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicketStatus>> GetStatusesAsync()
    {
        return await _context.Set<SupportTicketStatus>()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
    }

    public async Task<bool> CategoryExistsAsync(int id)
        => await _context.Set<SupportTicketCategory>().AnyAsync(l => l.Id == id);

    public async Task<bool> PriorityExistsAsync(int id)
        => await _context.Set<SupportTicketPriority>().AnyAsync(l => l.Id == id);

    public async Task<bool> StatusExistsAsync(int id)
        => await _context.Set<SupportTicketStatus>().AnyAsync(l => l.Id == id);
}
