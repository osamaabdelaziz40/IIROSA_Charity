using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Housing Project Repository Implementation
/// Provides data access operations for HousingProject entity
/// </summary>
public class HousingProjectRepository : Repository<HousingProject>, IHousingProjectRepository
{
    private readonly DbSet<HousingProject> _dbSet;

    public HousingProjectRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<HousingProject>();
    }

    #region Search and Filter

    public async Task<IEnumerable<HousingProject>> SearchAsync(string searchTerm)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted &&
                        (p.Name.Contains(searchTerm) ||
                         p.ProjectType.Contains(searchTerm) ||
                         (p.Description != null && p.Description.Contains(searchTerm)) ||
                         (p.Address != null && p.Address.Contains(searchTerm))))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<(IEnumerable<HousingProject> Items, int TotalCount)> GetFilteredAsync(
        string? name = null,
        string? projectType = null,
        string? projectStatus = null,
        int? countryId = null,
        int? regionId = null,
        int? centerId = null,
        Guid? charityId = null,
        string? housingType = null)
    {
        var query = IncludeNavigationProperties()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(projectType))
            query = query.Where(p => p.ProjectType == projectType);

        if (!string.IsNullOrWhiteSpace(projectStatus))
            query = query.Where(p => p.ProjectStatus == projectStatus);

        if (countryId.HasValue)
            query = query.Where(p => p.CountryId == countryId.Value);

        if (regionId.HasValue)
            query = query.Where(p => p.RegionId == regionId.Value);

        if (centerId.HasValue)
            query = query.Where(p => p.CenterId == centerId.Value);

        if (charityId.HasValue)
            query = query.Where(p => p.CharityId == charityId.Value);

        if (!string.IsNullOrWhiteSpace(housingType))
            query = query.Where(p => p.HousingType == housingType);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Status-based queries

    public async Task<IEnumerable<HousingProject>> GetActiveProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.ProjectStatus != "Completed" && p.ProjectStatus != "On Hold")
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetCompletedProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.ProjectStatus == "Completed")
            .OrderByDescending(p => p.ActualEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetDelayedProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted &&
                        p.ExpectedEndDate.HasValue &&
                        p.ActualEndDate.HasValue &&
                        p.ActualEndDate > p.ExpectedEndDate)
            .OrderBy(p => p.ActualEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetByStatusAsync(string status)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.ProjectStatus == status)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    #endregion

    #region Location-based queries

    public async Task<IEnumerable<HousingProject>> GetByCountryAsync(int countryId)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.CountryId == countryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetByRegionAsync(int regionId)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.RegionId == regionId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetByCenterAsync(int centerId)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.CenterId == centerId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetByCharityAsync(Guid charityId)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.CharityId == charityId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    #endregion

    #region Type-based queries

    public async Task<IEnumerable<HousingProject>> GetByProjectTypeAsync(string projectType)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.ProjectType == projectType)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetByHousingTypeAsync(string housingType)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.HousingType == housingType)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    #endregion

    #region Beneficiary queries

    public async Task<IEnumerable<HousingProject>> GetByFamilyAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.FamilyId == familyId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HousingProject>> GetUnassignedProjectsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && p.FamilyId == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    #endregion

    #region Specific queries

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Name == name && !p.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> IsActiveAsync(Guid id)
    {
        return await _dbSet
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => p.ProjectStatus != "Completed" && p.ProjectStatus != "On Hold")
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsCompletedAsync(Guid id)
    {
        return await _dbSet
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => p.ProjectStatus == "Completed")
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalProjectsAsync()
    {
        return await _dbSet.CountAsync(p => !p.IsDeleted);
    }

    public async Task<int> GetProjectsByStatusCountAsync(string status)
    {
        return await _dbSet.CountAsync(p => !p.IsDeleted && p.ProjectStatus == status);
    }

    public async Task<decimal> GetTotalBudgetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.ProjectStatus == status)
            .SumAsync(p => p.TotalBudget);
    }

    public async Task<double> GetAverageCompletionPercentageAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .AverageAsync(p => (double)p.CompletionPercentage);
    }

    public async Task<int> GetDelayedProjectsCountAsync()
    {
        return await _dbSet
            .CountAsync(p => !p.IsDeleted &&
                           p.ExpectedEndDate.HasValue &&
                           p.ActualEndDate.HasValue &&
                           p.ActualEndDate > p.ExpectedEndDate);
    }

    public async Task<int> GetOnTrackProjectsCountAsync()
    {
        return await _dbSet
            .CountAsync(p => !p.IsDeleted &&
                           p.CompletionPercentage > 0 &&
                           (!p.ActualEndDate.HasValue || !p.ExpectedEndDate.HasValue ||
                             p.ActualEndDate <= p.ExpectedEndDate));
    }

    public async Task<Dictionary<string, int>> GetProjectsByRegionAsync()
    {
        return await _dbSet
            .Include(p => p.Region)
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.Region != null ? p.Region.Name : "Unknown")
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetProjectsByCharityAsync()
    {
        return await _dbSet
            .Include(p => p.Charity)
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.Charity != null ? p.Charity.Name : "Unassigned")
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetProjectsByTypeAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.ProjectType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, decimal>> GetBudgetByStatusAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.ProjectStatus)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(p => p.TotalBudget));
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<HousingProject> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(p => p.Country)
            .Include(p => p.Region)
            .Include(p => p.Center)
            .Include(p => p.Charity)
            .Include(p => p.Family);
    }

    public System.Linq.IQueryable<HousingProject> IncludeFullDetails()
    {
        return IncludeNavigationProperties();
    }

    public async Task<(IEnumerable<HousingProject> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(p => !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<HousingProject> projects)
    {
        await _dbSet.AddRangeAsync(projects);
    }

    public void UpdateRange(IEnumerable<HousingProject> projects)
    {
        _dbSet.UpdateRange(projects);
    }

    public void DeleteRange(IEnumerable<HousingProject> projects)
    {
        _dbSet.RemoveRange(projects);
    }

    #endregion
}
