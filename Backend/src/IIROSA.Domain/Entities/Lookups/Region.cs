using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Region lookup entity (UC-14.7)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class Region : LookupEntity
{
    /// <summary>
    /// Region code (e.g., "Riyadh", "Makkah", "Madinah")
    /// </summary>
    public string? RegionCode { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Foreign key to Country
    /// </summary>
    public int? CountryId { get; set; }

    // Navigation properties
    public virtual Country? Country { get; set; }
    public virtual ICollection<Center> Centers { get; set; } = new List<Center>();
    public virtual ICollection<Charity> Charities { get; set; } = new List<Charity>();
}