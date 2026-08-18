using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Seasonal Aid Distribution Repository Implementation
/// Provides data access operations for SeasonalAidDistribution entity
/// </summary>
public class SeasonalAidDistributionRepository : Repository<SeasonalAidDistribution>, ISeasonalAidDistributionRepository
{
    private readonly DbSet<SeasonalAidDistribution> _dbSet;

    public SeasonalAidDistributionRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<SeasonalAidDistribution>();
    }

    #region Beneficiary-specific queries

    public async Task<IEnumerable<SeasonalAidDistribution>> GetByBeneficiaryAsync(Guid beneficiaryId)
    {
        return await IncludeNavigationProperties()
            .Where(d => d.BeneficiaryId == beneficiaryId && !d.IsDeleted)
            .OrderBy(d => d.DistributionDate)
            .ToListAsync();
    }

    public async Task<SeasonalAidDistribution?> GetLatestDistributionAsync(Guid beneficiaryId)
    {
        return await IncludeNavigationProperties()
            .Where(d => d.BeneficiaryId == beneficiaryId && !d.IsDeleted)
            .OrderByDescending(d => d.DistributionDate)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Campaign-specific queries

    public async Task<IEnumerable<SeasonalAidDistribution>> GetByCampaignAsync(Guid campaignId)
    {
        return await IncludeNavigationProperties()
            .Where(d => d.Beneficiary != null && d.Beneficiary.CampaignId == campaignId && !d.IsDeleted)
            .OrderBy(d => d.DistributionDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<SeasonalAidDistribution> Items, int TotalCount)> GetByCampaignPagedAsync(
        Guid campaignId, int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(d => d.Beneficiary != null && d.Beneficiary.CampaignId == campaignId && !d.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(d => d.DistributionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Date range queries

    public async Task<IEnumerable<SeasonalAidDistribution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await IncludeNavigationProperties()
            .Where(d => d.DistributionDate >= startDate && d.DistributionDate <= endDate && !d.IsDeleted)
            .OrderBy(d => d.DistributionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeasonalAidDistribution>> GetByCampaignAndDateRangeAsync(
        Guid campaignId, DateTime startDate, DateTime endDate)
    {
        return await IncludeNavigationProperties()
            .Where(d => d.Beneficiary != null &&
                        d.Beneficiary.CampaignId == campaignId &&
                        d.DistributionDate >= startDate &&
                        d.DistributionDate <= endDate &&
                        !d.IsDeleted)
            .OrderBy(d => d.DistributionDate)
            .ToListAsync();
    }

    #endregion

    #region Statistics

    public async Task<decimal> GetTotalDistributedAmountAsync(Guid campaignId)
    {
        return await _dbSet
            .Where(d => d.Beneficiary != null && d.Beneficiary.CampaignId == campaignId && !d.IsDeleted)
            .SumAsync(d => d.AmountDistributed);
    }

    public async Task<int> GetTotalDistributionsCountAsync(Guid campaignId)
    {
        return await _dbSet
            .Where(d => d.Beneficiary != null && d.Beneficiary.CampaignId == campaignId && !d.IsDeleted)
            .CountAsync();
    }

    public async Task<decimal> GetAverageDistributionAmountAsync(Guid campaignId)
    {
        return await _dbSet
            .Where(d => d.Beneficiary != null && d.Beneficiary.CampaignId == campaignId && !d.IsDeleted)
            .AverageAsync(d => d.AmountDistributed);
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<SeasonalAidDistribution> distributions)
    {
        await _dbSet.AddRangeAsync(distributions);
    }

    public void UpdateRange(IEnumerable<SeasonalAidDistribution> distributions)
    {
        _dbSet.UpdateRange(distributions);
    }

    public void DeleteRange(IEnumerable<SeasonalAidDistribution> distributions)
    {
        _dbSet.RemoveRange(distributions);
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<SeasonalAidDistribution> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(d => d.Beneficiary)
            .ThenInclude(b => b.Campaign)
            .Include(d => d.Beneficiary)
            .ThenInclude(b => b.Family);
    }

    public async Task<(IEnumerable<SeasonalAidDistribution> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = IncludeNavigationProperties()
            .Where(d => !d.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.DistributionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion
}
