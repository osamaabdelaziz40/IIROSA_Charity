using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Seasonal Aid Distribution Repository Interface
/// Provides data access methods for SeasonalAidDistribution entity
/// </summary>
public interface ISeasonalAidDistributionRepository : IRepository<SeasonalAidDistribution>
{
    // Extended queries
    Task<(IEnumerable<SeasonalAidDistribution> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    // Beneficiary-specific queries
    Task<IEnumerable<SeasonalAidDistribution>> GetByBeneficiaryAsync(Guid beneficiaryId);
    Task<SeasonalAidDistribution?> GetLatestDistributionAsync(Guid beneficiaryId);

    // Campaign-specific queries
    Task<IEnumerable<SeasonalAidDistribution>> GetByCampaignAsync(Guid campaignId);
    Task<(IEnumerable<SeasonalAidDistribution> Items, int TotalCount)> GetByCampaignPagedAsync(
        Guid campaignId, int pageNumber, int pageSize);

    // Date range queries
    Task<IEnumerable<SeasonalAidDistribution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<SeasonalAidDistribution>> GetByCampaignAndDateRangeAsync(
        Guid campaignId, DateTime startDate, DateTime endDate);

    // Statistics
    Task<decimal> GetTotalDistributedAmountAsync(Guid campaignId);
    Task<int> GetTotalDistributionsCountAsync(Guid campaignId);
    Task<decimal> GetAverageDistributionAmountAsync(Guid campaignId);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<SeasonalAidDistribution> distributions);
    void UpdateRange(IEnumerable<SeasonalAidDistribution> distributions);
    void DeleteRange(IEnumerable<SeasonalAidDistribution> distributions);

    // Include Operations
    System.Linq.IQueryable<SeasonalAidDistribution> IncludeNavigationProperties();
}
