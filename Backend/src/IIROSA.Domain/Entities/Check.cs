using Framework.Core.SharedServices.Entities;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Check entity - Represents a general check or payment instrument
/// Inherits from FullAuditedEntity<Guid>
/// Implements all use cases UC-11.1 through UC-11.10
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// IMPORTANT: Charity users CANNOT access this module. Only Admin, Super Admin, and Accountant can manage checks.
/// </summary>
public class Check : FullAuditedEntity
{
    // ========== Check Information (UC-11.1) ==========

    /// <summary>
    /// Check number (auto-generated or manual entry)
    /// </summary>
    public string CheckNumber { get; set; } = string.Empty;

    /// <summary>
    /// Check issue date (required)
    /// </summary>
    public DateTime CheckDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Check due date (optional, for post-dated checks)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Currency (EGP, SAR, USD)
    /// </summary>
    public string Currency { get; set; } = "EGP";

    // ========== Beneficiary Information (UC-11.2) ==========

    /// <summary>
    /// Beneficiary type (Individual, Company, Charity, Supplier, Employee)
    /// </summary>
    public string? BeneficiaryType { get; set; }

    /// <summary>
    /// Beneficiary name (required)
    /// </summary>
    public string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to ChequeBeneficiary lookup (optional, can be manual entry)
    /// </summary>
    public int? FK_ChequeBeneficiaryId { get; set; }

    /// <summary>
    /// Beneficiary address
    /// </summary>
    public string? BeneficiaryAddress { get; set; }

    /// <summary>
    /// Beneficiary phone
    /// </summary>
    public string? BeneficiaryPhone { get; set; }

    /// <summary>
    /// Beneficiary email
    /// </summary>
    public string? BeneficiaryEmail { get; set; }

    /// <summary>
    /// Beneficiary ID/Passport number
    /// </summary>
    public string? BeneficiaryIdNumber { get; set; }

    // ========== Financial Information (UC-11.3) ==========

    /// <summary>
    /// Check amount (required)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Amount in words (auto-generated or manual)
    /// </summary>
    public string? AmountInWords { get; set; }

    /// <summary>
    /// Payment reason (Salary, Supplier Refund, Expense, Other)
    /// </summary>
    public string? PaymentReason { get; set; }

    /// <summary>
    /// Payment description
    /// </summary>
    public string? PaymentDescription { get; set; }

    // ========== Bank Information ==========

    /// <summary>
    /// Foreign key to Bank lookup
    /// </summary>
    public int? FK_BankId { get; set; }

    /// <summary>
    /// Bank branch (optional)
    /// </summary>
    public string? BankBranch { get; set; }

    /// <summary>
    /// Bank account number
    /// </summary>
    public string? AccountNumber { get; set; }

    // ========== Status Information (UC-11.5, UC-11.6) ==========

    /// <summary>
    /// Check status (Pending, Issued, Cleared, Void)
    /// Default: Pending
    /// </summary>
    public string CheckStatus { get; set; } = "Pending";

    /// <summary>
    /// Issue date (when check was issued)
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Clearance date (when check was cleared by bank)
    /// </summary>
    public DateTime? ClearanceDate { get; set; }

    /// <summary>
    /// Bank reference for cleared check
    /// </summary>
    public string? BankReference { get; set; }

    /// <summary>
    /// Clearance notes
    /// </summary>
    public string? ClearanceNotes { get; set; }

    /// <summary>
    /// Void date (when check was voided)
    /// </summary>
    public DateTime? VoidDate { get; set; }

    /// <summary>
    /// Void reason (Lost, Stopped, Error, Expired, Other)
    /// </summary>
    public string? VoidReason { get; set; }

    /// <summary>
    /// Void notes (required when voiding)
    /// </summary>
    public string? VoidNotes { get; set; }

    // ========== Approval ==========

    /// <summary>
    /// Indicates if check requires approval
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// User who approved the check
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Approval date
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    // ========== Additional Information ==========

    /// <summary>
    /// Check image or scan
    /// </summary>
    public Guid? FK_CheckImageId { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to ChequeBeneficiary lookup
    /// </summary>
    public virtual ChequeBeneficiary? ChequeBeneficiary { get; set; }

    /// <summary>
    /// Navigation to Bank lookup
    /// </summary>
    public virtual Bank? Bank { get; set; }

    // ========== Computed Properties ==========

    /// <summary>
    /// Indicates if check can be modified (only pending checks)
    /// </summary>
    public bool CanModify => CheckStatus == "Pending";

    /// <summary>
    /// Indicates if check can be marked as cleared (issued checks only)
    /// </summary>
    public bool CanBeCleared => CheckStatus == "Issued";

    /// <summary>
    /// Indicates if check can be voided (pending or issued checks only)
    /// </summary>
    public bool CanBeVoided => CheckStatus == "Pending" || CheckStatus == "Issued";
}
