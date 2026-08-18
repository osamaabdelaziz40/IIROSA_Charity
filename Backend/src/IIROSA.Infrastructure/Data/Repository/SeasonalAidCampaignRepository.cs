using IIROSA.Application.DTOs.SeasonalAid;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Seasonal Aid Campaign Repository Implementation
/// Provides data access operations for SeasonalAidCampaign entity
/// </summary>
public class SeasonalAidCampaignRepository : Repository<SeasonalAidCampaign>, ISeasonalAidCampaignRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<SeasonalAidCampaign> _dbSet;

    public SeasonalAidCampaignRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<SeasonalAidCampaign>();
    }

    #region Search and Filter

    public async Task<IEnumerable<SeasonalAidCampaign>> SearchAsync(string searchTerm)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted &&
                        (c.Name.Contains(searchTerm) ||
                         c.CampaignType.Contains(searchTerm) ||
                         (c.Description != null && c.Description.Contains(searchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetActiveCampaignsAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.IsActive && !c.IsClosed)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetByCharityAsync(Guid charityId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CharityId == charityId)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetByCountryAsync(int countryId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CountryId == countryId)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetByRegionAsync(int regionId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.RegionId == regionId)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetByCenterAsync(int centerId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CenterId == centerId)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidCampaign>> GetByCampaignTypeAsync(string campaignType)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CampaignType == campaignType)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetFilteredAsync(SeasonalAidCampaignFilterDto filter)
    {
        var query = IncludeNavigationProperties()
            .Where(c => !c.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(filter.SearchTerm) ||
                c.CampaignType.Contains(filter.SearchTerm) ||
                (c.Description != null && c.Description.Contains(filter.SearchTerm)));
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.CampaignType))
        {
            query = query.Where(c => c.CampaignType == filter.CampaignType);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == filter.IsActive.Value);
        }

        if (filter.IsClosed.HasValue)
        {
            query = query.Where(c => c.IsClosed == filter.IsClosed.Value);
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(c => c.CountryId == filter.CountryId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            query = query.Where(c => c.RegionId == filter.RegionId.Value);
        }

        if (filter.CenterId.HasValue)
        {
            query = query.Where(c => c.CenterId == filter.CenterId.Value);
        }

        if (filter.CharityId.HasValue)
        {
            query = query.Where(c => c.CharityId == filter.CharityId.Value);
        }

        // Apply date range filters
        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(c => c.StartDate >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(c => c.StartDate <= filter.StartDateTo.Value);
        }

        if (filter.EndDateFrom.HasValue)
        {
            query = query.Where(c => c.EndDate >= filter.EndDateFrom.Value);
        }

        if (filter.EndDateTo.HasValue)
        {
            query = query.Where(c => c.EndDate <= filter.EndDateTo.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var sortDirection = filter.SortDescending ? "descending" : "ascending";
            try
            {
                query = query.OrderBy($"{filter.SortBy} {sortDirection}");
            }
            catch
            {
                query = query.OrderBy(c => c.Name);
            }
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Specific Queries

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<bool> IsActiveAsync(Guid id)
    {
        return await _dbSet
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => c.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsClosedAsync(Guid id)
    {
        return await _dbSet
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => c.IsClosed)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Statistics

    public async Task<int> GetBeneficiaryCountAsync(Guid campaignId)
    {
        return await _context.Set<SeasonalAidBeneficiary>()
            .CountAsync(b => b.CampaignId == campaignId && !b.IsDeleted);
    }

    public async Task<decimal> GetAllocatedBudgetAsync(Guid campaignId)
    {
        return await _context.Set<SeasonalAidBeneficiary>()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .SumAsync(b => b.AllocationAmount);
    }

    public async Task<decimal> GetDistributedBudgetAsync(Guid campaignId)
    {
        return await _context.Set<SeasonalAidBeneficiary>()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .SelectMany(b => b.Distributions)
            .SumAsync(d => d.AmountDistributed);
    }

    public async Task<int> GetDistributedBeneficiariesCountAsync(Guid campaignId)
    {
        return await _context.Set<SeasonalAidBeneficiary>()
            .CountAsync(b => b.CampaignId == campaignId && b.IsDistributed && !b.IsDeleted);
    }

    public async Task<Dictionary<string, int>> GetBeneficiariesByRegionAsync(Guid campaignId)
    {
        var beneficiaries = await _context.Set<SeasonalAidBeneficiary>()
            .Include(b => b.Family)
            .ThenInclude(f => f.City)
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .ToListAsync();

        return beneficiaries
            .GroupBy(b => b.Family != null && b.Family.City != null ? b.Family.City.Name : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetBeneficiariesByCharityAsync(Guid campaignId)
    {
        var beneficiaries = await _context.Set<SeasonalAidBeneficiary>()
            .Include(b => b.Family)
            .ThenInclude(f => f.Charity)
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .ToListAsync();

        return beneficiaries
            .GroupBy(b => b.Family != null && b.Family.Charity != null ? b.Family.Charity.Name : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<SeasonalAidCampaign> campaigns)
    {
        await _dbSet.AddRangeAsync(campaigns);
    }

    public void UpdateRange(IEnumerable<SeasonalAidCampaign> campaigns)
    {
        _dbSet.UpdateRange(campaigns);
    }

    public void DeleteRange(IEnumerable<SeasonalAidCampaign> campaigns)
    {
        _dbSet.RemoveRange(campaigns);
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<SeasonalAidCampaign> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(c => c.Country)
            .Include(c => c.Region)
            .Include(c => c.Center)
            .Include(c => c.Charity);
    }

    public System.Linq.IQueryable<SeasonalAidCampaign> IncludeBeneficiaries()
    {
        return IncludeNavigationProperties()
            .Include(c => c.Beneficiaries);
    }

    public System.Linq.IQueryable<SeasonalAidCampaign> IncludeFullDetails()
    {
        return IncludeNavigationProperties()
            .Include(c => c.Beneficiaries)
                .ThenInclude(b => b.Family)
            .Include(c => c.Beneficiaries)
                .ThenInclude(b => b.Distributions);
    }

    public async Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(c => !c.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Interface-compliant GetFilteredAsync method
    public async Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetFilteredAsync(
        string? name = null, string? campaignType = null, bool? isActive = null,
        bool? isClosed = null, int? countryId = null, int? regionId = null,
        int? centerId = null, Guid? charityId = null)
    {
        var query = IncludeNavigationProperties()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(campaignType))
            query = query.Where(c => c.CampaignType == campaignType);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (isClosed.HasValue)
            query = query.Where(c => c.IsClosed == isClosed.Value);

        if (countryId.HasValue)
            query = query.Where(c => c.CountryId == countryId.Value);

        if (regionId.HasValue)
            query = query.Where(c => c.RegionId == regionId.Value);

        if (centerId.HasValue)
            query = query.Where(c => c.CenterId == centerId.Value);

        if (charityId.HasValue)
            query = query.Where(c => c.CharityId == charityId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedOn)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion
}
