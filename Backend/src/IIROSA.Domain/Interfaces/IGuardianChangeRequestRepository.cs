using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// GuardianChangeRequest Repository Interface (UC-FAM-09/10). Paging, status/charity filters
/// and the pending-duplicate check compose over the generic queryable in the service layer —
/// same division of labour as the family repository.
/// </summary>
public interface IGuardianChangeRequestRepository : IRepository<GuardianChangeRequest>
{
}
