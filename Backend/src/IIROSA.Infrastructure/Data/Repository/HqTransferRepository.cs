using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// HqTransfer Repository Implementation (UC-TRF-01…08)
/// </summary>
public class HqTransferRepository : Repository<HqTransfer>, IHqTransferRepository
{
    private readonly DbSet<HqTransfer> _dbSet;

    public HqTransferRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<HqTransfer>();
    }

    /// <inheritdoc />
    public async Task<HqTransfer?> GetByIdWithLookupsAsync(Guid id)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<HqTransfer?> GetByIdWithLinesAsync(Guid id)
    {
        return await IncludeNavigationProperties()
            .Include(t => t.Details)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<HqTransfer> Items, int TotalCount)> GetTransfersPagedAsync(
        Expression<Func<HqTransfer, bool>>? filter = null,
        Func<IQueryable<HqTransfer>, IOrderedQueryable<HqTransfer>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = IncludeNavigationProperties().AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderByDescending(t => t.TransactionDate);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc />
    public IQueryable<HqTransfer> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(t => t.Country)
            .Include(t => t.Department);
    }
}
