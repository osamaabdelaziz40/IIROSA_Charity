using Framework.Core.Data;
using Framework.Core.SharedServices.Entities;
using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// ChequeBeneficiary lookup entity
/// Represents reusable check beneficiaries (UC-11.2)
/// Inherits from LookupEntityBase (int-based with audit fields)
/// </summary>
public class ChequeBeneficiary : LookupEntityBase
{
    /// <summary>
    /// Beneficiary type (Individual, Company, Charity, Supplier, Employee)
    /// </summary>
    public string? BeneficiaryType { get; set; }

    /// <summary>
    /// Beneficiary address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Beneficiary phone
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Beneficiary email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Beneficiary ID/Passport number
    /// </summary>
    public string? IdNumber { get; set; }

    /// <summary>
    /// Default bank for this beneficiary
    /// </summary>
    public int? FK_BankId { get; set; }

    /// <summary>
    /// Default account number
    /// </summary>
    public string? AccountNumber { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to Bank lookup
    /// </summary>
    public virtual Bank? Bank { get; set; }

    /// <summary>
    /// Collection of checks issued to this beneficiary
    /// </summary>
    public virtual ICollection<Check> Checks { get; set; } = new List<Check>();
}
