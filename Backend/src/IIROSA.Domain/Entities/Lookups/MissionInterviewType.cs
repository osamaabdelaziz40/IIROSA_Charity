using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Mission Interview Type lookup entity (UC-MSN-04)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class MissionInterviewType : LookupEntity
{
    /// <summary>
    /// Interview type code (e.g., "FIELD", "OFFICE", "PHONE")
    /// </summary>
    public string? TypeCode { get; set; }
}
