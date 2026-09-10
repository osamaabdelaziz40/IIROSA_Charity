using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Charity Repository Interface
/// Provides data access methods for Charity entity
/// </summary>
public interface ICharityRepository : IRepository<Charity>
{
    // Extended methods
    Task<Charity?> GetByCodeAsync(string code);
    Task<Charity?> GetByUserIdAsync(string userId);
    Task<bool> ExistsAsync(Guid id);

    // Search and Filter
    Task<IEnumerable<Charity>> SearchAsync(string searchTerm);
    Task<IEnumerable<Charity>> GetActiveCharitiesAsync();
    Task<IEnumerable<Charity>> GetByCountryAsync(int countryId);
    Task<IEnumerable<Charity>> GetByRegionAsync(int regionId);
    Task<IEnumerable<Charity>> GetByCenterAsync(int centerId);
    Task<IEnumerable<Charity>> GetFilteredAsync(string? name = null, string? code = null,
        int? countryId = null, int? regionId = null, int? centerId = null,
        bool? isActive = null, bool? isLocked = null);

    // Paginated version
    Task<(IEnumerable<Charity> Items, int TotalCount)> GetFilteredPaginatedAsync(
        string? searchTerm = null,
        int? countryId = null,
        int? regionId = null,
        int? centerId = null,
        bool? isActive = null,
        bool? isLocked = null,
        bool? isAddEnabled = null,
        bool? isUpdateEnabled = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        // Appended rather than inserted: every parameter here is optional, so a new one in the
        // middle silently shifts any positional caller.
        Guid? charityId = null);

    // Specific Queries
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    // Statistics
    Task<int> GetFamilyCountAsync(Guid charityId);
    Task<int> GetOrphanCountAsync(Guid charityId);
    Task<int> GetSponsorCountAsync(Guid charityId);

    /// <summary>
    /// Register-level aggregate for the UC-3.10 statistics band — grouped passes over the
    /// scoped live rows rather than one count query per card. <paramref name="charityId"/>
    /// and <paramref name="countryId"/> narrow the same way the paged list's filters do.
    /// </summary>
    Task<(int Total, int Active, int Locked, int ReceivingDonations, int AddedThisMonth,
        List<(int CountryId, string? NameAr, string? NameEn, int Count)> ByCountry)>
        GetRegisterStatisticsAsync(int? countryId = null, Guid? charityId = null);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<Charity> charities);
    void UpdateRange(IEnumerable<Charity> charities);
    void DeleteRange(IEnumerable<Charity> charities);

    // Include Operations
    System.Linq.IQueryable<Charity> IncludeNavigationProperties();
}
