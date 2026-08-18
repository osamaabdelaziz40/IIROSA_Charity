using IIROSA.Application.DTOs.SeasonalAid;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Seasonal Aid Beneficiary Repository Implementation
/// Provides data access operations for SeasonalAidBeneficiary entity
/// </summary>
public class SeasonalAidBeneficiaryRepository : Repository<SeasonalAidBeneficiary>, ISeasonalAidBeneficiaryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<SeasonalAidBeneficiary> _dbSet;

    public SeasonalAidBeneficiaryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<SeasonalAidBeneficiary>();
    }

    #region Campaign-specific queries

    public async Task<SeasonalAidBeneficiary?> GetByCampaignAndFamilyAsync(Guid campaignId, Guid familyId)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(b => b.CampaignId == campaignId && b.FamilyId == familyId && !b.IsDeleted);
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetByCampaignAsync(Guid campaignId)
    {
        return await IncludeDistributions()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .OrderBy(b => b.RegistrationDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<SeasonalAidBeneficiary> Items, int TotalCount)> GetByCampaignFilteredAsync(
        Guid campaignId, SeasonalAidBeneficiaryFilterDto filter)
    {
        var query = IncludeDistributions()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted);

        // Apply filters
        if (filter.IsDistributed.HasValue)
        {
            query = query.Where(b => b.IsDistributed == filter.IsDistributed.Value);
        }

        if (filter.CharityId.HasValue)
        {
            query = query.Where(b => b.Family != null && b.Family.FK_CharityId == filter.CharityId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            query = query.Where(b => b.Family != null && b.Family.CityId == filter.RegionId.Value);
        }

        if (filter.CenterId.HasValue)
        {
            query = query.Where(b => b.Family != null && b.Family.CityId == filter.CenterId.Value);
        }

        // Apply date range filters
        if (filter.RegistrationDateFrom.HasValue)
        {
            query = query.Where(b => b.RegistrationDate >= filter.RegistrationDateFrom.Value);
        }

        if (filter.RegistrationDateTo.HasValue)
        {
            query = query.Where(b => b.RegistrationDate <= filter.RegistrationDateTo.Value);
        }

        if (filter.DistributionDateFrom.HasValue)
        {
            query = query.Where(b => b.DistributionDate >= filter.DistributionDateFrom.Value);
        }

        if (filter.DistributionDateTo.HasValue)
        {
            query = query.Where(b => b.DistributionDate <= filter.DistributionDateTo.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(b =>
                (b.Family != null && b.Family.Code.Contains(filter.SearchTerm)) ||
                (b.Family != null && b.Family.Address != null && b.Family.Address.Contains(filter.SearchTerm)));
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
                query = query.OrderBy(b => b.RegistrationDate);
            }
        }
        else
        {
            query = query.OrderBy(b => b.RegistrationDate);
        }

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetByCampaignWithDistributionsAsync(Guid campaignId)
    {
        return await IncludeDistributions()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .OrderBy(b => b.RegistrationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetDistributedBeneficiariesAsync(Guid campaignId)
    {
        return await IncludeDistributions()
            .Where(b => b.CampaignId == campaignId && b.IsDistributed && !b.IsDeleted)
            .OrderBy(b => b.DistributionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetPendingBeneficiariesAsync(Guid campaignId)
    {
        return await IncludeDistributions()
            .Where(b => b.CampaignId == campaignId && !b.IsDistributed && !b.IsDeleted)
            .OrderBy(b => b.RegistrationDate)
            .ToListAsync();
    }

    #endregion

    #region Family-specific queries

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetByFamilyAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .Where(b => b.FamilyId == familyId && !b.IsDeleted)
            .OrderBy(b => b.RegistrationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetEligibleFamiliesAsync(EligibleFamiliesFilterDto filter)
    {
        // This method returns families that are eligible but NOT yet registered
        var campaign = await _context.Set<SeasonalAidCampaign>()
            .FirstOrDefaultAsync(c => c.Id == filter.CampaignId);

        if (campaign == null)
        {
            return Enumerable.Empty<SeasonalAidBeneficiary>();
        }

        // Get families that match criteria but are not already registered
        var familiesQuery = _context.Set<Family>()
            .Include(f => f.Charity)
            .Include(f => f.City)
            .Where(f => !f.IsDeleted && f.IsActive);

        // Apply filters based on campaign criteria
        if (campaign.CharityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.FK_CharityId == campaign.CharityId.Value);
        }

        if (filter.CharityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.FK_CharityId == filter.CharityId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.RegionId.Value);
        }

        if (filter.CenterId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.CenterId.Value);
        }

        // Apply search
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            familiesQuery = familiesQuery.Where(f =>
                f.Code.Contains(filter.SearchTerm) ||
                (f.Address != null && f.Address.Contains(filter.SearchTerm)));
        }

        // Exclude already registered families
        var registeredFamilyIds = await _dbSet
            .Where(b => b.CampaignId == filter.CampaignId && !b.IsDeleted)
            .Select(b => b.FamilyId)
            .ToListAsync();

        if (registeredFamilyIds.Any())
        {
            familiesQuery = familiesQuery.Where(f => !registeredFamilyIds.Contains(f.Id));
        }

        var families = await familiesQuery
            .OrderBy(f => f.Code)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Convert to SeasonalAidBeneficiary objects (not saved, just for display)
        return families.Select(f => new SeasonalAidBeneficiary
        {
            Id = Guid.Empty,
            CampaignId = filter.CampaignId,
            FamilyId = f.Id,
            AllocationAmount = campaign.PerFamilyAllocation,
            Currency = campaign.BudgetCurrency,
            IsRegistered = false,
            RegistrationDate = DateTime.UtcNow
        });
    }

    #endregion

    #region Status checks

    public async Task<bool> IsFamilyRegisteredAsync(Guid campaignId, Guid familyId)
    {
        return await _dbSet
            .AnyAsync(b => b.CampaignId == campaignId && b.FamilyId == familyId && !b.IsDeleted);
    }

    public async Task<bool> IsDistributedAsync(Guid beneficiaryId)
    {
        return await _dbSet
            .Where(b => b.Id == beneficiaryId && !b.IsDeleted)
            .Select(b => b.IsDistributed)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalBeneficiariesAsync(Guid campaignId)
    {
        return await _dbSet
            .CountAsync(b => b.CampaignId == campaignId && !b.IsDeleted);
    }

    public async Task<decimal> GetTotalAllocationAsync(Guid campaignId)
    {
        return await _dbSet
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .SumAsync(b => b.AllocationAmount);
    }

    public async Task<decimal> GetTotalDistributedAsync(Guid campaignId)
    {
        return await _dbSet
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted)
            .SelectMany(b => b.Distributions)
            .SumAsync(d => d.AmountDistributed);
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<SeasonalAidBeneficiary> beneficiaries)
    {
        await _dbSet.AddRangeAsync(beneficiaries);
    }

    public void UpdateRange(IEnumerable<SeasonalAidBeneficiary> beneficiaries)
    {
        _dbSet.UpdateRange(beneficiaries);
    }

    public void DeleteRange(IEnumerable<SeasonalAidBeneficiary> beneficiaries)
    {
        _dbSet.RemoveRange(beneficiaries);
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<SeasonalAidBeneficiary> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(b => b.Campaign)
            .Include(b => b.Family)
            .Include(b => b.Family.Charity)
            .Include(b => b.Family.City);
    }

    public System.Linq.IQueryable<SeasonalAidBeneficiary> IncludeDistributions()
    {
        return IncludeNavigationProperties()
            .Include(b => b.Distributions);
    }

    public async Task<(IEnumerable<SeasonalAidBeneficiary> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(b => !b.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.RegistrationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Interface-compliant methods
    public async Task<(IEnumerable<SeasonalAidBeneficiary> Items, int TotalCount)> GetByCampaignFilteredAsync(
        Guid campaignId, bool? isDistributed = null, Guid? charityId = null,
        int? regionId = null, int? centerId = null)
    {
        var query = IncludeNavigationProperties()
            .Where(b => b.CampaignId == campaignId && !b.IsDeleted);

        if (isDistributed.HasValue)
            query = query.Where(b => b.IsDistributed == isDistributed.Value);

        if (charityId.HasValue)
            query = query.Where(b => b.Family != null && b.Family.CharityId == charityId.Value);

        if (regionId.HasValue)
            query = query.Where(b => b.Family != null && b.Family.CityId == regionId.Value);

        if (centerId.HasValue)
            query = query.Where(b => b.Family != null && b.Family.CityId == centerId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.RegistrationDate)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<SeasonalAidBeneficiary>> GetEligibleFamiliesAsync(
        Guid campaignId, int? regionId = null, int? centerId = null,
        Guid? charityId = null, string? searchTerm = null)
    {
        // This method returns families that are eligible but NOT yet registered
        var campaign = await _context.Set<SeasonalAidCampaign>()
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
        {
            return Enumerable.Empty<SeasonalAidBeneficiary>();
        }

        // Get families that match criteria but are not already registered
        System.Linq.IQueryable<Family> familiesQuery = _context.Set<Family>()
            .Include(f => f.Charity)
            .Include(f => f.City);

        // Apply filters based on campaign criteria
        if (campaign.CharityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CharityId == campaign.CharityId.Value);
        }

        if (charityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CharityId == charityId.Value);
        }

        if (regionId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == regionId.Value);
        }

        if (centerId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == centerId.Value);
        }

        // Apply search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            familiesQuery = familiesQuery.Where(f =>
                f.Code.Contains(searchTerm) ||
                (f.Address != null && f.Address.Contains(searchTerm)));
        }

        // Exclude already registered families
        var registeredFamilyIds = await _dbSet
            .Where(b => b.CampaignId == campaignId)
            .Select(b => b.FamilyId)
            .ToListAsync();

        if (registeredFamilyIds.Any())
        {
            familiesQuery = familiesQuery.Where(f => !registeredFamilyIds.Contains(f.Id));
        }

        var families = await familiesQuery
            .OrderBy(f => f.Code)
            .Take(100) // Limit results
            .ToListAsync();

        // Convert to SeasonalAidBeneficiary objects (not saved, just for display)
        return families.Select(f => new SeasonalAidBeneficiary
        {
            Id = Guid.Empty,
            CampaignId = campaignId,
            FamilyId = f.Id,
            AllocationAmount = campaign.PerFamilyAllocation,
            Currency = campaign.BudgetCurrency,
            IsRegistered = false,
            RegistrationDate = DateTime.UtcNow
        });
    }

    #endregion
}
