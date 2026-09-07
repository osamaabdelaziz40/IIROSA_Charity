using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Orphan Repository Interface
/// Provides data access methods for Orphan entity
/// </summary>
public interface IOrphanRepository : IRepository<Orphan>
{
    /// <summary>
    /// Get orphan by code
    /// </summary>
    Task<Orphan?> GetByCodeAsync(string code);

    /// <summary>
    /// Check if code is unique
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    /// <summary>
    /// Search orphans by name or code
    /// </summary>
    Task<(IEnumerable<Orphan> Items, int TotalCount)> SearchFilteredAsync(
        string? searchTerm = null,
        Guid? charityId = null,
        int? regionId = null,
        int? centerId = null,
        string? sponsorshipStatus = null,
        int? ageFrom = null,
        int? ageTo = null,
        string? gender = null,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Get orphans by charity
    /// </summary>
    Task<IEnumerable<Orphan>> GetByCharityIdAsync(int? charityId);

    /// <summary>
    /// Get orphans by family
    /// </summary>
    Task<IEnumerable<Orphan>> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Get sponsored orphans
    /// </summary>
    Task<IEnumerable<Orphan>> GetSponsoredOrphansAsync();

    /// <summary>
    /// Get unsponsored orphans
    /// </summary>
    Task<IEnumerable<Orphan>> GetUnsponsoredOrphansAsync();

    /// <summary>
    /// Include navigation properties
    /// </summary>
    System.Linq.IQueryable<Orphan> IncludeNavigationProperties();

    /// <summary>
    /// Get active orphans
    /// </summary>
    Task<IEnumerable<Orphan>> GetActiveOrphansAsync();

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null);

    /// <summary>
    /// Get paginated orphans with filtering
    /// </summary>
    Task<(IEnumerable<Orphan> Items, int TotalCount)> GetPagedAsync(
        System.Linq.Expressions.Expression<System.Func<Orphan, bool>>? filter = null,
        int pageNumber = 1,
        int pageSize = 10);
}
