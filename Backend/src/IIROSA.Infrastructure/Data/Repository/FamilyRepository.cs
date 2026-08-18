using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Family Repository Implementation
/// Provides data access operations for Family entity
/// </summary>
public class FamilyRepository : Repository<Family>, IFamilyRepository
{
    private readonly DbSet<Family> _dbSet;

    public FamilyRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Family>();
    }

    public async Task<Family?> GetByCodeAsync(string code)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Code == code && !f.IsDeleted);
    }

    #region Search and Filter

    public async Task<IEnumerable<Family>> SearchAsync(string searchTerm)
    {
        return await IncludeNavigationProperties()
            .Where(f => !f.IsDeleted &&
                        (f.Code.Contains(searchTerm) ||
                         (f.Address != null && f.Address.Contains(searchTerm)) ||
                         (f.HeadOfFamily != null && f.HeadOfFamily.Contains(searchTerm))))
            .OrderBy(f => f.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Family>> GetActiveFamiliesAsync()
    {
        return await IncludeNavigationProperties()
            .Where(f => !f.IsDeleted && f.IsActive)
            .OrderBy(f => f.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Family>> GetByCharityAsync(Guid charityId)
    {
        return await IncludeNavigationProperties()
            .Where(f => !f.IsDeleted && f.FK_CharityId == charityId)
            .OrderBy(f => f.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Family>> GetByRegionAsync(int regionId)
    {
        return await IncludeNavigationProperties()
            .Where(f => !f.IsDeleted && f.CityId == regionId)
            .OrderBy(f => f.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Family>> GetByCityAsync(int cityId)
    {
        return await IncludeNavigationProperties()
            .Where(f => !f.IsDeleted && f.CityId == cityId)
            .OrderBy(f => f.Code)
            .ToListAsync();
    }

    #endregion

    #region Specific Queries

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = _dbSet.Where(f => f.Code == code && !f.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(f => f.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(f => f.Id == id && !f.IsDeleted);
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<Family> families)
    {
        await _dbSet.AddRangeAsync(families);
    }

    public void UpdateRange(IEnumerable<Family> families)
    {
        _dbSet.UpdateRange(families);
    }

    public void DeleteRange(IEnumerable<Family> families)
    {
        _dbSet.RemoveRange(families);
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<Family> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(f => f.Country)
            .Include(f => f.City)
            .Include(f => f.Charity)
            .Include(f => f.Orphans);
    }

    public async Task<(IEnumerable<Family> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(f => !f.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(f => f.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion
}
