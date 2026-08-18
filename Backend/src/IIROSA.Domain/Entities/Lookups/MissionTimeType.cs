using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Mission Time Type lookup entity (UC-8.4)
/// Inherits from LookupEntity which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class MissionTimeType : LookupEntity
{
    /// <summary>
    /// Mission time type code (e.g., "ONETIME", "DAILY", "WEEKLY", "MONTHLY")
    /// </summary>
    public string? TimeTypeCode { get; set; }

    /// <summary>
    /// Description of the mission time type
    /// </summary>
    public string? TypeDescription { get; set; }

    // Navigation properties
    public virtual ICollection<Mission> Missions { get; set; } = new List<Mission>();
}
