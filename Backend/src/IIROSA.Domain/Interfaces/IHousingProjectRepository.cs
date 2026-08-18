using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Housing Project Repository Interface
/// Defines data access operations for HousingProject entity
/// </summary>
public interface IHousingProjectRepository : IRepository<HousingProject>
{
    // Search and Filter
    Task<IEnumerable<HousingProject>> SearchAsync(string searchTerm);
    Task<(IEnumerable<HousingProject> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<HousingProject> Items, int TotalCount)> GetFilteredAsync(
        string? name = null,
        string? projectType = null,
        string? projectStatus = null,
        int? countryId = null,
        int? regionId = null,
        int? centerId = null,
        Guid? charityId = null,
        string? housingType = null);

    // Status-based queries
    Task<IEnumerable<HousingProject>> GetActiveProjectsAsync();
    Task<IEnumerable<HousingProject>> GetCompletedProjectsAsync();
    Task<IEnumerable<HousingProject>> GetDelayedProjectsAsync();
    Task<IEnumerable<HousingProject>> GetByStatusAsync(string status);

    // Location-based queries
    Task<IEnumerable<HousingProject>> GetByCountryAsync(int countryId);
    Task<IEnumerable<HousingProject>> GetByRegionAsync(int regionId);
    Task<IEnumerable<HousingProject>> GetByCenterAsync(int centerId);
    Task<IEnumerable<HousingProject>> GetByCharityAsync(Guid charityId);

    // Type-based queries
    Task<IEnumerable<HousingProject>> GetByProjectTypeAsync(string projectType);
    Task<IEnumerable<HousingProject>> GetByHousingTypeAsync(string housingType);

    // Beneficiary queries
    Task<IEnumerable<HousingProject>> GetByFamilyAsync(Guid familyId);
    Task<IEnumerable<HousingProject>> GetUnassignedProjectsAsync();

    // Specific queries
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsActiveAsync(Guid id);
    Task<bool> IsCompletedAsync(Guid id);

    // Statistics
    Task<int> GetTotalProjectsAsync();
    Task<int> GetProjectsByStatusCountAsync(string status);
    Task<decimal> GetTotalBudgetByStatusAsync(string status);
    Task<double> GetAverageCompletionPercentageAsync();
    Task<int> GetDelayedProjectsCountAsync();
    Task<int> GetOnTrackProjectsCountAsync();
    Task<Dictionary<string, int>> GetProjectsByRegionAsync();
    Task<Dictionary<string, int>> GetProjectsByCharityAsync();
    Task<Dictionary<string, int>> GetProjectsByTypeAsync();
    Task<Dictionary<string, decimal>> GetBudgetByStatusAsync();

    // Bulk operations
    Task AddRangeAsync(IEnumerable<HousingProject> projects);
    void UpdateRange(IEnumerable<HousingProject> projects);
    void DeleteRange(IEnumerable<HousingProject> projects);

    // Include operations
    System.Linq.IQueryable<HousingProject> IncludeNavigationProperties();
    System.Linq.IQueryable<HousingProject> IncludeFullDetails();
}
