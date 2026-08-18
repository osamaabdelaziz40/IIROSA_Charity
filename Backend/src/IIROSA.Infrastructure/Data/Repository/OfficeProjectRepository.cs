using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// OfficeProject Repository Implementation
/// Provides data access operations for OfficeProject entity using ApplicationDbContext
/// Implements all use cases UC-7.1 through UC-7.14
/// </summary>
public class OfficeProjectRepository : Repository<OfficeProject>, IOfficeProjectRepository
{
    private readonly DbSet<OfficeProject> _dbSet;

    public OfficeProjectRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<OfficeProject>();
    }

    // ========== OfficeProject-Specific Queries ==========

    public async Task<IEnumerable<OfficeProject>> GetByCharityIdAsync(Guid charityId)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.FK_CharityId == charityId)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetByStatusAsync(bool isFinished)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.IsFinished == isFinished)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetByProjectTypeAsync(int projectTypeId)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.FK_OfficeProjectTypeId == projectTypeId)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.ProjectDate >= startDate && p.ProjectDate <= endDate)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetOngoingProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsFinished)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetCompletedProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => p.IsFinished)
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetByLocationAsync(int? countryId, int? regionId, int? centerId)
    {
        var query = IncludeNavigationProperties().AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(p => p.FK_CountryId == countryId.Value);
        }

        if (regionId.HasValue)
        {
            query = query.Where(p => p.FK_RegionId == regionId.Value);
        }

        if (centerId.HasValue)
        {
            query = query.Where(p => p.FK_CenterId == centerId.Value);
        }

        return await query
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfficeProject>> GetByDonorAsync(string donorName)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.DonorName.Contains(donorName))
            .OrderByDescending(p => p.ProjectDate)
            .ToListAsync();
    }

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

    public IQueryable<OfficeProject> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(p => p.OfficeProjectType)
            .Include(p => p.Country)
            .Include(p => p.Region)
            .Include(p => p.Center)
            .Include(p => p.Charity);
    }

    public IQueryable<OfficeProject> IncludeSpecificNavigationProperties(params Expression<Func<OfficeProject, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
