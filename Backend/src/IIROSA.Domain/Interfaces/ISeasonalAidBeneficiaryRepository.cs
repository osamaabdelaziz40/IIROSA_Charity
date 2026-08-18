using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Seasonal Aid Beneficiary Repository Interface
/// Provides data access methods for SeasonalAidBeneficiary entity
/// </summary>
public interface ISeasonalAidBeneficiaryRepository : IRepository<SeasonalAidBeneficiary>
{
    // Extended queries
    Task<SeasonalAidBeneficiary?> GetByCampaignAndFamilyAsync(Guid campaignId, Guid familyId);
    Task<(IEnumerable<SeasonalAidBeneficiary> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    // Campaign-specific queries (UC-9.7)
    Task<IEnumerable<SeasonalAidBeneficiary>> GetByCampaignAsync(Guid campaignId);
    Task<(IEnumerable<SeasonalAidBeneficiary> Items, int TotalCount)> GetByCampaignFilteredAsync(
        Guid campaignId, bool? isDistributed = null, Guid? charityId = null,
        int? regionId = null, int? centerId = null);
    Task<IEnumerable<SeasonalAidBeneficiary>> GetByCampaignWithDistributionsAsync(Guid campaignId);
    Task<IEnumerable<SeasonalAidBeneficiary>> GetDistributedBeneficiariesAsync(Guid campaignId);
    Task<IEnumerable<SeasonalAidBeneficiary>> GetPendingBeneficiariesAsync(Guid campaignId);

    // Family-specific queries
    Task<IEnumerable<SeasonalAidBeneficiary>> GetByFamilyAsync(Guid familyId);
    Task<IEnumerable<SeasonalAidBeneficiary>> GetEligibleFamiliesAsync(
        Guid campaignId, int? regionId = null, int? centerId = null,
        Guid? charityId = null, string? familyType = null);

    // Status checks
    Task<bool> IsFamilyRegisteredAsync(Guid campaignId, Guid familyId);
    Task<bool> IsDistributedAsync(Guid beneficiaryId);

    // Statistics
    Task<int> GetTotalBeneficiariesAsync(Guid campaignId);
    Task<decimal> GetTotalAllocationAsync(Guid campaignId);
    Task<decimal> GetTotalDistributedAsync(Guid campaignId);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<SeasonalAidBeneficiary> beneficiaries);
    void UpdateRange(IEnumerable<SeasonalAidBeneficiary> beneficiaries);
    void DeleteRange(IEnumerable<SeasonalAidBeneficiary> beneficiaries);

    // Include Operations
    System.Linq.IQueryable<SeasonalAidBeneficiary> IncludeNavigationProperties();
    System.Linq.IQueryable<SeasonalAidBeneficiary> IncludeDistributions();
}
