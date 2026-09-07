using Framework.Core.SharedServices.Entities;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Check entity — a general cheque issued outside the orphan payment cycle (chapter 16, UC-CHQ).
/// Inherits from FullAuditedEntity (Guid); all audit fields come from the base class.
/// Tenancy: every cheque is owned by the charity it was issued for (FK_CharityId); the
/// application service scopes reads and writes to that charity from the caller's token.
/// </summary>
public class Check : FullAuditedEntity
{
    /// <summary>
    /// Cheque number as printed on the instrument (mandatory, UC-CHQ-02).
    /// Unique per (number, bank, charity) among live rows — cheque books are per bank account.
    /// </summary>
    public string CheckNumber { get; set; } = string.Empty;

    /// <summary>
    /// Cheque date تاريخ الشيك (mandatory, UC-CHQ-02).
    /// </summary>
    public DateTime CheckDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Currency ISO code driving the amount and the Arabic words conversion (mandatory, UC-CHQ-06).
    /// </summary>
    public string Currency { get; set; } = "EGP";

    // ========== Beneficiary (UC-CHQ-05) ==========

    /// <summary>
    /// Optional link to the reusable beneficiary catalogue row this cheque was issued to.
    /// The name/details below are the snapshot printed on the cheque.
    /// </summary>
    public int? FK_ChequeBeneficiaryId { get; set; }

    /// <summary>
    /// Beneficiary type (Individual, Company, Charity, Supplier, Employee).
    /// </summary>
    public string? BeneficiaryType { get; set; }

    /// <summary>
    /// Beneficiary name as printed on the cheque (mandatory, UC-CHQ-02).
    /// </summary>
    public string BeneficiaryName { get; set; } = string.Empty;

    public string? BeneficiaryAddress { get; set; }

    public string? BeneficiaryPhone { get; set; }

    public string? BeneficiaryEmail { get; set; }

    public string? BeneficiaryIdNumber { get; set; }

    // ========== Financial (UC-CHQ-07) ==========

    /// <summary>
    /// Cheque amount (mandatory, UC-CHQ-02).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Amount in Arabic words تفقيط — generated on save by the service and printed on the cheque face.
    /// </summary>
    public string? AmountInWords { get; set; }

    // ========== Bank ==========

    /// <summary>
    /// The bank the cheque is drawn on (mandatory, UC-CHQ-02; also drives the print positions, UC-CHQ-08).
    /// </summary>
    public int? FK_BankId { get; set; }

    public string? BankBranch { get; set; }

    public string? AccountNumber { get; set; }

    // ========== Tenancy ==========

    /// <summary>
    /// The charity this cheque was issued for. Stamped server-side from the creating user's
    /// token; a charity user can only see and affect their own cheques (UC-CHQ-01/03 AC-3).
    /// </summary>
    public Guid? FK_CharityId { get; set; }

    // ========== Screen flags (§16.S.2) ==========

    /// <summary>
    /// شيك تالف — the cheque was damaged/voided on the paper register.
    /// </summary>
    public bool IsDamaged { get; set; }

    /// <summary>
    /// تم رد الشيك — the cheque was returned by the bank/beneficiary.
    /// </summary>
    public bool IsReturned { get; set; }

    /// <summary>
    /// تم الصرف — the cheque was disbursed/cashed.
    /// </summary>
    public bool IsDispensed { get; set; }

    /// <summary>
    /// Fourth unlabelled checkbox of the legacy add/edit screen (CheckDone).
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// Register filter of the statement screen (§16.S.3): شيكات إيتام (Orphans) or شيكات أفراد (Individuals).
    /// General cheques default to Individuals.
    /// </summary>
    public string ChequeType { get; set; } = "Individuals";

    /// <summary>
    /// تعليقات — free comment on the cheque.
    /// </summary>
    public string? Notes { get; set; }

    // ========== Navigation Properties ==========

    public virtual ChequeBeneficiary? ChequeBeneficiary { get; set; }

    public virtual Bank? Bank { get; set; }

    public virtual Charity? Charity { get; set; }
}
