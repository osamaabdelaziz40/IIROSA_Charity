using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// GuardianChangeRequest Repository Implementation (UC-FAM-09/10)
/// Registered by the Infrastructure assembly scan (any class ending in "Repository"),
/// same as the rest of the repositories — no manual DI line needed.
/// </summary>
public class GuardianChangeRequestRepository : Repository<GuardianChangeRequest>, IGuardianChangeRequestRepository
{
    public GuardianChangeRequestRepository(ApplicationDbContext context) : base(context)
    {
    }
}
