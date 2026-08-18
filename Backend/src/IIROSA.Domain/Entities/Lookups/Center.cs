using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Center lookup entity (UC-14.6)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class Center : LookupEntity
{
    /// <summary>
    /// Center code (e.g., "CEN-001", "RYD-001")
    /// </summary>
    public string? CenterCode { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Foreign key to Region
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Foreign key to Country (auto-populated from region)
    /// </summary>
    public int? CountryId { get; set; }

    // Navigation properties
    public virtual Region? Region { get; set; }
    public virtual Country? Country { get; set; }
    public virtual ICollection<Charity> Charities { get; set; } = new List<Charity>();
}