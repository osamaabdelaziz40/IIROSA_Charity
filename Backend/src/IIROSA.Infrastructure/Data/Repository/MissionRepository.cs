using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Mission Repository Implementation
/// Provides data access operations for Mission entity using ApplicationDbContext
/// Implements all use cases UC-8.1 through UC-8.13
/// </summary>
public class MissionRepository : Repository<Mission>, IMissionRepository
{
    private readonly DbSet<Mission> _dbSet;

    public MissionRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Mission>();
    }

    // ========== Mission-Specific Queries ==========

    public async Task<IEnumerable<Mission>> GetByUserIdAsync(Guid userId)
    {
        return await IncludeNavigationProperties()
            .Where(m => m.FK_UserId == userId)
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetByStatusAsync(bool isCompleted)
    {
        return await IncludeNavigationProperties()
            .Where(m => m.IsMissionCompleted == isCompleted)
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetByMissionTypeAsync(int missionTypeId)
    {
        return await IncludeNavigationProperties()
            .Where(m => m.FK_MissionTypeId == missionTypeId)
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetByMissionTimeTypeAsync(int missionTimeTypeId)
    {
        return await IncludeNavigationProperties()
            .Where(m => m.FK_MissionTimeTypeId == missionTimeTypeId)
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await IncludeNavigationProperties()
            .Where(m => m.MissionDate >= startDate && m.MissionDate <= endDate)
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetOverdueMissionsAsync()
    {
        var today = DateTime.Today;
        return await IncludeNavigationProperties()
            .Where(m => !m.IsMissionCompleted && m.MissionDate < today)
            .OrderBy(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetByLocationAsync(int? countryId, int? regionId, int? centerId)
    {
        var query = IncludeNavigationProperties().AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(m => m.FK_CountryId == countryId.Value);
        }

        if (regionId.HasValue)
        {
            query = query.Where(m => m.FK_RegionId == regionId.Value);
        }

        if (centerId.HasValue)
        {
            query = query.Where(m => m.FK_CenterId == centerId.Value);
        }

        return await query
            .OrderByDescending(m => m.MissionDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Mission> Items, int TotalCount)> GetMissionsPagedAsync(
        Expression<Func<Mission, bool>>? filter = null,
        Func<IQueryable<Mission>, IOrderedQueryable<Mission>>? orderBy = null,
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
            query = query.OrderByDescending(m => m.MissionDate);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public IQueryable<Mission> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(m => m.MissionType)
            .Include(m => m.MissionTimeType)
            .Include(m => m.Country)
            .Include(m => m.Region)
            .Include(m => m.Center);
    }

    public IQueryable<Mission> IncludeSpecificNavigationProperties(params Expression<Func<Mission, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
