using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// FamilyCharityTransfer Repository Interface (UC-FAM-06)
/// </summary>
public interface IFamilyCharityTransferRepository : IRepository<FamilyCharityTransfer>
{
    /// <summary>
    /// Get the transfer history of one family, newest first — the movement trail behind the
    /// family file (who moved it, when, from where, to where)
    /// </summary>
    Task<IEnumerable<FamilyCharityTransfer>> GetByFamilyIdAsync(Guid familyId);
}
