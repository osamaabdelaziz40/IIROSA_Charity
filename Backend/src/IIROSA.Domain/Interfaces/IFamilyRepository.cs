using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Family Repository Interface
/// Provides data access methods for Family entity
/// </summary>
public interface IFamilyRepository : IRepository<Family>
{
    // Basic CRUD - Extended methods
    Task<Family?> GetByCodeAsync(string code);
    Task<(IEnumerable<Family> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    // Search and Filter
    Task<IEnumerable<Family>> SearchAsync(string searchTerm);
    Task<IEnumerable<Family>> GetActiveFamiliesAsync();
    Task<IEnumerable<Family>> GetByCharityAsync(Guid charityId);
    Task<IEnumerable<Family>> GetByRegionAsync(int regionId);
    Task<IEnumerable<Family>> GetByCityAsync(int cityId);

    // Specific Queries
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<Family> families);
    void UpdateRange(IEnumerable<Family> families);
    void DeleteRange(IEnumerable<Family> families);

    // Include Operations
    System.Linq.IQueryable<Family> IncludeNavigationProperties();
}
