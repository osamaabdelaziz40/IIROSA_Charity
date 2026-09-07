using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Seasonal Aid Campaign Repository Interface
/// Provides data access methods for SeasonalAidCampaign entity
/// </summary>
public interface ISeasonalAidCampaignRepository : IRepository<SeasonalAidCampaign>
{
    // Extended queries
    Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    // Search and Filter (UC-9.6)
    Task<IEnumerable<SeasonalAidCampaign>> SearchAsync(string searchTerm);
    Task<IEnumerable<SeasonalAidCampaign>> GetActiveCampaignsAsync();
    Task<IEnumerable<SeasonalAidCampaign>> GetByCountryAsync(int countryId);
    Task<IEnumerable<SeasonalAidCampaign>> GetByRegionAsync(int regionId);
    Task<IEnumerable<SeasonalAidCampaign>> GetByCenterAsync(int centerId);
    Task<IEnumerable<SeasonalAidCampaign>> GetByCampaignTypeAsync(string campaignType);
    Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetFilteredAsync(
        string? name = null, string? campaignType = null, bool? isActive = null,
        bool? isClosed = null, int? countryId = null, int? regionId = null,
        int? centerId = null);

    // Paged variant with the full filter surface. The DTO-typed overload lives on the concrete
    // class only — Domain interfaces cannot reference Application DTOs — so callers go through
    // these primitives, named-argument style, same as ICharityRepository.GetFilteredPaginatedAsync.
    Task<(IEnumerable<SeasonalAidCampaign> Items, int TotalCount)> GetFilteredPaginatedAsync(
        string? searchTerm = null, string? campaignType = null, bool? isActive = null,
        bool? isClosed = null, int? countryId = null, int? regionId = null,
        int? centerId = null,
        DateTime? startDateFrom = null, DateTime? startDateTo = null,
        DateTime? endDateFrom = null, DateTime? endDateTo = null,
        int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool sortDescending = false);

    // Specific Queries
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsActiveAsync(Guid id);
    Task<bool> IsClosedAsync(Guid id);

    // Statistics (UC-9.10)
    Task<int> GetBeneficiaryCountAsync(Guid campaignId);
    Task<decimal> GetAllocatedBudgetAsync(Guid campaignId);
    Task<decimal> GetDistributedBudgetAsync(Guid campaignId);
    Task<int> GetDistributedBeneficiariesCountAsync(Guid campaignId);
    Task<Dictionary<string, int>> GetBeneficiariesByRegionAsync(Guid campaignId);
    Task<Dictionary<string, int>> GetBeneficiariesByCharityAsync(Guid campaignId);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<SeasonalAidCampaign> campaigns);
    void UpdateRange(IEnumerable<SeasonalAidCampaign> campaigns);
    void DeleteRange(IEnumerable<SeasonalAidCampaign> campaigns);

    // Include Operations
    System.Linq.IQueryable<SeasonalAidCampaign> IncludeNavigationProperties();
    System.Linq.IQueryable<SeasonalAidCampaign> IncludeBeneficiaries();
    System.Linq.IQueryable<SeasonalAidCampaign> IncludeFullDetails();
}
