using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;

namespace IIROSA.Application.Services;

/// <summary>
/// Office Project Type Service
/// Inherits from LookupServiceBase for common lookup operations
/// Implements UC-7.1: Office Project Type management
/// </summary>
public class OfficeProjectTypeService : LookupServiceBase<OfficeProjectType, LookupDto, CreateLookupDto, UpdateLookupDto>, IOfficeProjectTypeService
{
    public OfficeProjectTypeService(
        IOfficeProjectTypeRepository repository,
        AutoMapper.IMapper mapper,
        Microsoft.Extensions.Logging.ILogger<OfficeProjectTypeService> logger)
        : base(repository, mapper, logger)
    {
    }
}
