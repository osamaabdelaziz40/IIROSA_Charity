using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Father Repository Interface
/// Provides data access methods for Father entity
/// Implements UC-4.2 (Add Family Father) and UC-4.9 (Update Father Details)
/// </summary>
public interface IFatherRepository : IRepository<Father>
{
    /// <summary>
    /// Get father by family ID
    /// </summary>
    Task<Father?> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null);

    /// <summary>
    /// Include navigation properties
    /// </summary>
    System.Linq.IQueryable<Father> IncludeNavigationProperties();
}
