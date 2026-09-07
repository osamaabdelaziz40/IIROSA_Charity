using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// OfficeProject Repository Implementation (UC-OFP-01…06)
/// </summary>
public class OfficeProjectRepository : Repository<OfficeProject>, IOfficeProjectRepository
{
    private readonly DbSet<OfficeProject> _dbSet;

    public OfficeProjectRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<OfficeProject>();
    }

    /// <inheritdoc />
    public async Task<OfficeProject?> GetByIdWithDetailsAsync(Guid id)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<OfficeProject> Items, int TotalCount)> GetProjectsPagedAsync(
        Expression<Func<OfficeProject, bool>>? filter = null,
        Func<IQueryable<OfficeProject>, IOrderedQueryable<OfficeProject>>? orderBy = null,
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
            query = query.OrderByDescending(p => p.ProjectDate);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc />
    public IQueryable<OfficeProject> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(p => p.OfficeProjectType)
            .Include(p => p.Country)
            .Include(p => p.Region)
            .Include(p => p.Center)
            .Include(p => p.Charity);
    }
}
