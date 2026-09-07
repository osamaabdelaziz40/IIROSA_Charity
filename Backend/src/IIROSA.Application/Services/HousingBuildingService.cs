using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Housing Building Service (UC-HOU-05) — rides the generic lookup machinery
/// </summary>
public class HousingBuildingService : LookupServiceBase<HousingBuilding, LookupDto, CreateLookupDto, UpdateLookupDto>, IHousingBuildingService
{
    public HousingBuildingService(
        ILookupRepository<HousingBuilding> repository,
        IMapper mapper,
        ILogger<HousingBuildingService> logger) : base(repository, mapper, logger)
    {
    }
}
