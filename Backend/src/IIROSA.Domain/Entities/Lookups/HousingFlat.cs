using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Housing flat lookup (UC-HOU-05 · §11.S.2 رقم الشقه) — a flat inside a
/// <see cref="HousingBuilding"/>. The flat's display name (its number) lives in
/// NameAr/NameEn; <see cref="Number"/> carries it as an integer for grids/sorting.
/// LookupEntity carries no parent field, so the building link is an own column here.
/// </summary>
public class HousingFlat : LookupEntity
{
    /// <summary>
    /// Flat number (رقم الشقه) — e.g. 12
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Flat size in square metres
    /// </summary>
    public int? SizeInMtr { get; set; }

    /// <summary>
    /// Free-text description of the flat
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Owning building (رقم العماره) — required
    /// </summary>
    public int BuildingId { get; set; }

    public virtual HousingBuilding Building { get; set; } = null!;
}
