using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// OrphanPaymentItem Repository Implementation
/// Provides data access operations for OrphanPaymentItem entity
/// </summary>
public class OrphanPaymentItemRepository : Repository<OrphanPaymentItem>, IOrphanPaymentItemRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<OrphanPaymentItem> _dbSet;

    public OrphanPaymentItemRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<OrphanPaymentItem>();
    }

    #region Basic CRUD - Extended Methods

    public async Task<IEnumerable<OrphanPaymentItem>> GetByPaymentGroupIdAsync(Guid orphanPaymentId)
    {
        return await _dbSet
            .Include(opi => opi.Orphan)
            .ThenInclude(o => o.Family)
            .Where(opi => opi.OrphanPaymentId == orphanPaymentId && !opi.IsDeleted)
            .OrderBy(opi => opi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrphanPaymentItem>> GetByOrphanIdAsync(Guid orphanId)
    {
        return await _dbSet
            .Include(opi => opi.OrphanPayment)
            .Where(opi => opi.OrphanId == orphanId && !opi.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid orphanPaymentId, Guid orphanId)
    {
        return await _dbSet
            .AnyAsync(opi => opi.OrphanPaymentId == orphanPaymentId &&
                            opi.OrphanId == orphanId &&
                            !opi.IsDeleted);
    }

    public async Task RemoveAllFromGroupAsync(Guid orphanPaymentId)
    {
        var items = await _dbSet
            .Where(opi => opi.OrphanPaymentId == orphanPaymentId && !opi.IsDeleted)
            .ToListAsync();

        foreach (var item in items)
        {
            _dbSet.Remove(item);
        }
    }

    public async Task<OrphanPaymentItem?> GetWithOrphanAsync(Guid id)
    {
        return await _dbSet
            .Include(opi => opi.Orphan)
            .ThenInclude(o => o.Family)
            .Include(opi => opi.OrphanPayment)
            .FirstOrDefaultAsync(opi => opi.Id == id && !opi.IsDeleted);
    }

    public async Task<IEnumerable<OrphanPaymentItem>> GetWithOrphansByGroupAsync(Guid orphanPaymentId)
    {
        return await _dbSet
            .Include(opi => opi.Orphan)
            .ThenInclude(o => o.Family)
            .Include(opi => opi.OrphanPayment)
            .Where(opi => opi.OrphanPaymentId == orphanPaymentId && !opi.IsDeleted)
            .OrderBy(opi => opi.DisplayOrder)
            .ToListAsync();
    }

    #endregion
}
