using Framework.Identity.Data.Entities;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Incoming ↔ Employee link repository (epic 16, UC-COR-09).
/// Identity users are read through the remapped Users table (see ApplicationDbContext) —
/// flat projections only, never tracked ApplicationUser entities.
/// </summary>
public class IncomingEmployeeRepository : Repository<IncomingEmployee>, IIncomingEmployeeRepository
{
    private readonly DbSet<IncomingEmployee> _dbSet;
    private readonly ApplicationDbContext _context;

    public IncomingEmployeeRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<IncomingEmployee>();
        _context = context;
    }

    public async Task<IEnumerable<IncomingEmployee>> GetByIncomingAsync(Guid incomingId)
    {
        // Soft delete is NOT a global query filter in this platform — detached
        // (soft-deleted) links stay out of the attached grid (review P1)
        return await _dbSet
            .Include(e => e.User)
            .Where(e => e.IncomingId == incomingId && !e.IsDeleted)
            .OrderBy(e => e.CreatedOn)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid incomingId, Guid userId)
    {
        // Only live links count — a detached employee can be re-attached (review P1)
        return await _dbSet.AnyAsync(e => e.IncomingId == incomingId && e.UserId == userId && !e.IsDeleted);
    }

    public async Task<IEnumerable<EmployeeCandidateRow>> GetAvailableEmployeesAsync(Guid incomingId, Guid? charityId)
    {
        var linkedUserIds = await _dbSet
            .Where(e => e.IncomingId == incomingId && !e.IsDeleted)
            .Select(e => e.UserId)
            .ToListAsync();

        var query = _context.Set<ApplicationUser>()
            .Where(u => u.IsActive && !linkedUserIds.Contains(u.Id));

        if (charityId.HasValue)
        {
            // §21.U.9 / 16-9 Task 2 — candidates come from the letter's charity only;
            // a NULL charity (HQ-owned letter, D1 decision) offers all active users
            query = query.Where(u => u.CharityId == charityId.Value);
        }

        return await query
            .OrderBy(u => u.FullName)
            .Select(u => new EmployeeCandidateRow
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email
            })
            .ToListAsync();
    }
}
