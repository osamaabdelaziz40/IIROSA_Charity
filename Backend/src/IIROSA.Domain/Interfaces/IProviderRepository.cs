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
    /// Get provider by family ID — the PRIMARY guardian (deterministic first live row)
    /// </summary>
    Task<Provider?> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// All live providers of a family (§11.S.2 multi-guardian), primary-first order
    /// </summary>
    Task<List<Provider>> GetAllByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null);

    /// <summary>
    /// Include navigation properties
    /// </summary>
    System.Linq.IQueryable<Provider> IncludeNavigationProperties();
}
