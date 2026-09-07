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

    /// <summary>
    /// Maximum HQ transfer amount for this destination country (UC-TRF-06/07).
    /// NULL = unlimited — never treat NULL as zero. The column is written only through
    /// 17-7's max-amount endpoint, not through the country CRUD.
    /// </summary>
    public decimal? MaxTransferAmount { get; set; }

    // ========== National-ID validation rules (UC-SYS-11, epic 19) ==========
    // Optional per country: NULL means the country sets no NID format rule and NID inputs
    // stay free-form. These are UX pre-validation only — server-side controls (19-12
    // uniqueness, service validators) remain authoritative.

    /// <summary>
    /// Regex the country's national id must match (e.g. Egyptian 14-digit numeric "^\d{14}$").
    /// NULL = no pattern rule.
    /// </summary>
    public string? NationalIdPattern { get; set; }

    /// <summary>
    /// Exact length of the country's national id (e.g. 14 for Egypt). NULL = no length rule.
    /// </summary>
    public int? NationalIdLength { get; set; }

    // Navigation properties
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    public virtual ICollection<Center> Centers { get; set; } = new List<Center>();
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
