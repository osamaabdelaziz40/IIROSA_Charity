using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Project Type lookup entity (UC-14.11)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class ProjectType : LookupEntity
{
    /// <summary>
    /// Project type code (e.g., "HOUSING", "EDUCATION", "HEALTH")
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// Description of project type
    /// </summary>
    public string? TypeDescription { get; set; }
}