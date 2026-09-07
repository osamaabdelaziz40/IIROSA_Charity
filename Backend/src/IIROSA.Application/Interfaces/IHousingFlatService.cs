using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Housing Flat Service Interface (UC-HOU-05 · §11.S.2 رقم الشقه)
/// Extends ILookupService for common lookup operations — rich DTOs so lookup
/// management can maintain Number / SizeInMtr / Description / BuildingId
/// </summary>
public interface IHousingFlatService : ILookupService<HousingFlat, HousingFlatDto, CreateHousingFlatDto, UpdateHousingFlatDto>
{
    /// <summary>
    /// Flats of one building (the §11.S.2 on-change cascade from رقم العماره, and the
    /// lookup-management flat screen). includeInactive widens it for management reads.
    /// </summary>
    Task<List<HousingFlatDto>> GetFlatsByBuildingAsync(int buildingId, bool includeInactive = false);
}
