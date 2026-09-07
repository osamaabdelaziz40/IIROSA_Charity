using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Housing building lookup (UC-HOU-05 · §11.S.2 رقم العماره) — an organisation-owned
/// building housing welfare families. Distinct from HousingType, which is the family
/// living-condition lookup (epic 5) and stays untouched.
/// HQ-maintained catalogue, NOT per-charity rows (decision recorded in 6-5).
/// The display title (e.g. "العماره رقم 7") lives in NameAr/NameEn.
/// </summary>
public class HousingBuilding : LookupEntity
{
    /// <summary>
    /// Building number (رقم العماره) — e.g. 7
    /// </summary>
    public int? BuildingNumber { get; set; }

    /// <summary>
    /// Building address (مدينه نصر) — was Location, renamed to the §11.S.2 column name
    /// </summary>
    public string? BuildingAddress { get; set; }

    /// <summary>
    /// Free-text description of the building
    /// </summary>
    public string? BuildingDescription { get; set; }

    public virtual ICollection<HousingFlat> Flats { get; set; } = new List<HousingFlat>();
}
