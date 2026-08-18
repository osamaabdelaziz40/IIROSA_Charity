using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Country lookup entity (UC-14.8)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class Country : LookupEntity
{
    /// <summary>
    /// ISO country code (e.g., "SA", "US", "EG")
    /// </summary>
    public string? IsoCode { get; set; }

    /// <summary>
    /// International dialing code (e.g., "+966", "+1", "+20")
    /// </summary>
    public string? DialingCode { get; set; }

    /// <summary>
    /// Currency used in country (e.g., "SAR", "USD", "EGP")
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Path to flag icon file
    /// </summary>
    public string? FlagIcon { get; set; }

    // Navigation properties
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    public virtual ICollection<Center> Centers { get; set; } = new List<Center>();
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
