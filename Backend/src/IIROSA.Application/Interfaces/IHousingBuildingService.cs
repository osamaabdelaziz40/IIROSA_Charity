using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Housing Building Service Interface (UC-HOU-05 · §11.S.2 رقم العماره)
/// Extends ILookupService for common lookup operations
/// </summary>
public interface IHousingBuildingService : ILookupService<HousingBuilding, LookupDto, CreateLookupDto, UpdateLookupDto>
{
}
