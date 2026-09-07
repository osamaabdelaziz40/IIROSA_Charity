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

    // ========== Cheque print positions (UC-CHQ-08) ==========
    // Coordinate offsets, in millimetres from the top-right edge of the cheque leaf, at which
    // each field is printed so it lands on the bank's pre-printed stationery. All nullable:
    // a bank without configured positions falls back to the default layout, and the client
    // is told there is nothing to align to rather than receiving fake coordinates.

    /// <summary>Date field offset (تاريخ الشيك).</summary>
    public decimal? ChequeDateX { get; set; }
    /// <summary>Date field offset (تاريخ الشيك).</summary>
    public decimal? ChequeDateY { get; set; }

    /// <summary>Beneficiary name offset (اسم المستفيد).</summary>
    public decimal? PayeeX { get; set; }
    /// <summary>Beneficiary name offset (اسم المستفيد).</summary>
    public decimal? PayeeY { get; set; }

    /// <summary>Amount in digits offset (المبلغ).</summary>
    public decimal? AmountX { get; set; }
    /// <summary>Amount in digits offset (المبلغ).</summary>
    public decimal? AmountY { get; set; }

    /// <summary>Amount in Arabic words offset (المبلغ بالحروف).</summary>
    public decimal? AmountWordsX { get; set; }
    /// <summary>Amount in Arabic words offset (المبلغ بالحروف).</summary>
    public decimal? AmountWordsY { get; set; }
}