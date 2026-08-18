using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// ChequeBeneficiary Repository Interface
/// Provides data access operations for ChequeBeneficiary lookup entity
/// </summary>
public interface IChequeBeneficiaryRepository : IRepository<ChequeBeneficiary>
{
    Task<IEnumerable<ChequeBeneficiary>> GetActiveAsync();
    Task<IEnumerable<ChequeBeneficiary>> SearchByNameAsync(string name);
    Task<IEnumerable<ChequeBeneficiary>> GetByTypeAsync(string beneficiaryType);
}
