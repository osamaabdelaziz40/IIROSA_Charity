using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// NGO Type lookup entity (UC-14.13)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class NGOType : LookupEntity
{
    /// <summary>
    /// NGO/Charity type code (e.g., "LOCAL", "INTERNATIONAL", "GOV")
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// Description of NGO type
    /// </summary>
    public string? TypeDescription { get; set; }
}