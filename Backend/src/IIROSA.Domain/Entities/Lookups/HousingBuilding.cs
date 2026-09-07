using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Housing building lookup (UC-HOU-05 · §11.S.2 رقم العماره) — an organisation-owned
/// building housing welfare families. Distinct from HousingType, which is the family
/// living-condition lookup (epic 5) and stays untouched.
/// HQ-maintained catalogue, NOT per-charity rows (decision recorded in 6-5).
/// </summary>
public class HousingBuilding : LookupEntity
{
    /// <summary>
    /// Address/district hint shown beside the building name (optional)
    /// </summary>
    public string? Location { get; set; }

    public virtual ICollection<HousingFlat> Flats { get; set; } = new List<HousingFlat>();
}
