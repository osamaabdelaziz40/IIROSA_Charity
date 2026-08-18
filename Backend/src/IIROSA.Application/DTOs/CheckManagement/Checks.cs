namespace IIROSA.Application.DTOs.CheckManagement;

/// <summary>
/// Check list item for grid display (UC-11.7)
/// </summary>
public class CheckListDto
{
    public Guid Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CheckStatus { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Check detail with all information (UC-11.8)
/// </summary>
public class CheckDetailDto
{
    public Guid Id { get; set; }

    // Check Information
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = string.Empty;

    // Beneficiary Information
    public int? FK_ChequeBeneficiaryId { get; set; }
    public string? BeneficiaryType { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? BeneficiaryAddress { get; set; }
    public string? BeneficiaryPhone { get; set; }
    public string? BeneficiaryEmail { get; set; }
    public string? BeneficiaryIdNumber { get; set; }

    // Financial Information
    public decimal Amount { get; set; }
    public string? AmountInWords { get; set; }
    public string? PaymentReason { get; set; }
    public string? PaymentDescription { get; set; }

    // Bank Information
    public int? FK_BankId { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }

    // Status Information
    public string CheckStatus { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ClearanceDate { get; set; }
    public string? BankReference { get; set; }
    public string? ClearanceNotes { get; set; }
    public DateTime? VoidDate { get; set; }
    public string? VoidReason { get; set; }
    public string? VoidNotes { get; set; }

    // Approval
    public bool RequiresApproval { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovalDate { get; set; }

    // Additional
    public string? Notes { get; set; }

    // Computed Properties
    public bool CanModify { get; set; }
    public bool CanBeCleared { get; set; }
    public bool CanBeVoided { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatorName { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public string? ModifierName { get; set; }
}

/// <summary>
/// Create check DTO (UC-11.1)
/// </summary>
public class CreateCheckDto
{
    // Required fields
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;

    // Optional fields
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? BeneficiaryType { get; set; }
    public int? FK_ChequeBeneficiaryId { get; set; }
    public string? BeneficiaryAddress { get; set; }
    public string? BeneficiaryPhone { get; set; }
    public string? BeneficiaryEmail { get; set; }
    public string? BeneficiaryIdNumber { get; set; }
    public string? AmountInWords { get; set; }
    public string? PaymentReason { get; set; }
    public string? PaymentDescription { get; set; }
    public int? FK_BankId { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }
    public bool RequiresApproval { get; set; } = false;
    public string? Notes { get; set; }
}

/// <summary>
/// Update check DTO
/// </summary>
public class UpdateCheckDto
{
    public string? CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Currency { get; set; }
    public string? BeneficiaryType { get; set; }
    public string? BeneficiaryName { get; set; }
    public int? FK_ChequeBeneficiaryId { get; set; }
    public string? BeneficiaryAddress { get; set; }
    public string? BeneficiaryPhone { get; set; }
    public string? BeneficiaryEmail { get; set; }
    public string? BeneficiaryIdNumber { get; set; }
    public decimal? Amount { get; set; }
    public string? AmountInWords { get; set; }
    public string? PaymentReason { get; set; }
    public string? PaymentDescription { get; set; }
    public int? FK_BankId { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Check filter DTO for queries (UC-11.7)
/// </summary>
public class CheckFilterDto
{
    public string? SearchText { get; set; }               // Search by check number or beneficiary
    public string? CheckStatus { get; set; }               // Filter by status
    public int? FK_BankId { get; set; }                    // Filter by bank
    public string? Currency { get; set; }                  // Filter by currency
    public DateTime? StartDate { get; set; }               // Date range filter
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }                // Amount range filter
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Paged result wrapper for checks
/// </summary>
public class CheckPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
}

/// <summary>
/// Check status summary (UC-11.10)
/// </summary>
public class CheckStatusSummaryDto
{
    public int TotalChecks { get; set; }
    public int PendingChecks { get; set; }
    public int IssuedChecks { get; set; }
    public int ClearedChecks { get; set; }
    public int VoidedChecks { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ClearedAmount { get; set; }
    public Dictionary<string, decimal> AmountByCurrency { get; set; } = new();
    public Dictionary<string, int> CountByBank { get; set; } = new();
}

// ========== DTOs for Specific Use Cases ==========

/// <summary>
/// Set check amount DTO (UC-11.3)
/// </summary>
public class SetCheckAmountDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? AmountInWords { get; set; }
}

/// <summary>
/// Set check date DTO (UC-11.4)
/// </summary>
public class SetCheckDateDto
{
    public DateTime CheckDate { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Mark check as cleared DTO (UC-11.5)
/// </summary>
public class MarkCheckClearedDto
{
    public DateTime ClearanceDate { get; set; } = DateTime.Today;
    public string? BankReference { get; set; }
    public string? ClearanceNotes { get; set; }
}

/// <summary>
/// Void check DTO (UC-11.6)
/// </summary>
public class VoidCheckDto
{
    public string VoidReason { get; set; } = string.Empty; // Lost, Stopped, Error, Expired, Other
    public DateTime VoidDate { get; set; } = DateTime.Today;
    public string VoidNotes { get; set; } = string.Empty;
}

/// <summary>
/// Reconciliation result (UC-11.9)
/// </summary>
public class CheckReconciliationDto
{
    public List<Guid> ReconciledCheckIds { get; set; } = new();
    public int TotalChecksReconciled { get; set; }
    public decimal TotalAmountReconciled { get; set; }
    public int UnreconciledChecks { get; set; }
    public DateTime ReconciliationDate { get; set; } = DateTime.Today;
    public string? ReconciliationNotes { get; set; }
}

/// <summary>
/// Check report filter (UC-11.10)
/// </summary>
public class CheckReportFilterDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CheckStatus { get; set; }
    public int? FK_BankId { get; set; }
    public string? Currency { get; set; }
    public string GroupBy { get; set; } = "Status"; // Status, Bank, Beneficiary
}

/// <summary>
/// Check report (UC-11.10)
/// </summary>
public class CheckReportDto
{
    public CheckStatusSummaryDto Summary { get; set; } = new();
    public List<CheckListDto> DetailedChecks { get; set; } = new();
    public List<CheckListDto> ClearedChecks { get; set; } = new();
    public List<CheckListDto> PendingChecks { get; set; } = new();
    public List<CheckListDto> VoidChecks { get; set; } = new();
    public DateTime ReportGeneratedOn { get; set; } = DateTime.UtcNow;
}
