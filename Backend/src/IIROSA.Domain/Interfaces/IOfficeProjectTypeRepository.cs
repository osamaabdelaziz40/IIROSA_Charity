using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OfficeProjectType Repository Interface
/// Defines data access operations for OfficeProjectType lookup entity
/// Implements use case UC-7.1: Office Project Type management
/// </summary>
public interface IOfficeProjectTypeRepository : ILookupRepository<OfficeProjectType>
{
}
