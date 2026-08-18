using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Mission Type lookup entity (UC-14.10)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class MissionType : LookupEntity
{
    /// <summary>
    /// Mission type code (e.g., "FIELD", "OFFICE", "MEDICAL")
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// Description of mission type
    /// </summary>
    public string? TypeDescription { get; set; }
}