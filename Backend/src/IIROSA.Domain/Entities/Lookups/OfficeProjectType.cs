using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Office Project Type lookup entity (UC-7.1)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class OfficeProjectType : LookupEntity
{
    /// <summary>
    /// Office project type code (e.g., "CONSTRUCTION", "EQUIPMENT", "TRAINING")
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// Description of office project type
    /// </summary>
    public string? TypeDescription { get; set; }
}
