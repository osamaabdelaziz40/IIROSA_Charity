using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Housing flat lookup (UC-HOU-05 · §11.S.2 رقم الشقه) — a flat inside a
/// <see cref="HousingBuilding"/>. The flat number lives in Name/NameAr (e.g. "شقة ١٢").
/// LookupEntity carries no parent field, so the building link is an own column here.
/// </summary>
public class HousingFlat : LookupEntity
{
    /// <summary>
    /// Owning building (رقم العماره) — required
    /// </summary>
    public int BuildingId { get; set; }

    public virtual HousingBuilding Building { get; set; } = null!;
}
