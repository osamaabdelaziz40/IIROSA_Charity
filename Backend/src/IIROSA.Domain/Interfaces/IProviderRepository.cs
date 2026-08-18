using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Provider Repository Interface
/// Provides data access methods for Provider entity (non-parent guardian)
/// Implements UC-4.6 (Add Non-Parent Provider)
/// </summary>
public interface IProviderRepository : IRepository<Provider>
{
    /// <summary>
    /// Get provider by family ID
    /// </summary>
    Task<Provider?> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null);

    /// <summary>
    /// Include navigation properties
    /// </summary>
    System.Linq.IQueryable<Provider> IncludeNavigationProperties();
}
