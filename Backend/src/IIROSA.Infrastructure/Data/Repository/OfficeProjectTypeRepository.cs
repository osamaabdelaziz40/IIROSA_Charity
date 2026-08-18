using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// OfficeProjectType Repository Implementation
/// Provides data access operations for OfficeProjectType lookup entity
/// Implements use case UC-7.1: Office Project Type management
/// </summary>
public class OfficeProjectTypeRepository : LookupRepository<OfficeProjectType>, IOfficeProjectTypeRepository
{
    public OfficeProjectTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}
