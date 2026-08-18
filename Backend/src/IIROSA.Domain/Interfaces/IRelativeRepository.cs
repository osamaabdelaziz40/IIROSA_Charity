using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Relative Repository Interface
/// Defines operations for managing Relative entities
/// </summary>
public interface IRelativeRepository : IRepository<Relative>
{
    /// <summary>
    /// Get all relatives for a specific family
    /// </summary>
    /// <param name="familyId">Family ID</param>
    /// <returns>List of relatives</returns>
    Task<IEnumerable<Relative>> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Get a specific relative by ID
    /// </summary>
    /// <param name="id">Relative ID</param>
    /// <returns>Relative entity or null</returns>
    Task<Relative?> GetByIdAsync(Guid id);

    /// <summary>
    /// Check if relative belongs to a family
    /// </summary>
    /// <param name="relativeId">Relative ID</param>
    /// <param name="familyId">Family ID</param>
    /// <returns>True if relative belongs to family</returns>
    Task<bool> BelongsToFamilyAsync(Guid relativeId, Guid familyId);
}
