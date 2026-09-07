using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Housing Flat Service Interface (UC-HOU-05 · §11.S.2 رقم الشقه)
/// Extends ILookupService for common lookup operations
/// </summary>
public interface IHousingFlatService : ILookupService<HousingFlat, LookupDto, CreateLookupDto, UpdateLookupDto>
{
    /// <summary>
    /// Active flats of one building (the §11.S.2 on-change cascade from رقم العماره)
    /// </summary>
    Task<List<LookupDto>> GetFlatsByBuildingAsync(int buildingId);
}
