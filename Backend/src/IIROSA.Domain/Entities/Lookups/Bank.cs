using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Bank lookup entity (UC-14.12)
/// Inherits from LookupEntityBase which provides:
/// - Id: int (inherited)
/// - Name, NameAr, NameEn, Description, IsActive (inherited)
/// - Audit fields (inherited from FullAuditedEntityBaseInt)
/// </summary>
public class Bank : LookupEntity
{
    /// <summary>
    /// Bank code (e.g., "RJHI0100", "NBPA0100")
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// SWIFT code for international transfers
    /// </summary>
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Bank branch address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Bank phone number
    /// </summary>
    public string? Phone { get; set; }
}