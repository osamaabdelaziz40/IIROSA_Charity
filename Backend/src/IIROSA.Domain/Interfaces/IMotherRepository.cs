using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Mother Repository Interface
/// Provides data access methods for Mother entity
/// Implements UC-4.3 (Add Family Mother) and UC-4.10 (Update Mother Details)
/// </summary>
public interface IMotherRepository : IRepository<Mother>
{
    /// <summary>
    /// Get mother by family ID
    /// </summary>
    Task<Mother?> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null);

    /// <summary>
    /// Include navigation properties
    /// </summary>
    System.Linq.IQueryable<Mother> IncludeNavigationProperties();
}
