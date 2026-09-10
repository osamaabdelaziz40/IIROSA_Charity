using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Family Phone Repository Interface
/// Provides data access methods for the FamilyPhone entity (family contact numbers)
/// </summary>
public interface IFamilyPhoneRepository : IRepository<FamilyPhone>
{
    /// <summary>
    /// All live phone rows of a family, default-first
    /// </summary>
    Task<List<FamilyPhone>> GetAllByFamilyIdAsync(Guid familyId);
}
