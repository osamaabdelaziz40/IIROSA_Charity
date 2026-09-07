using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// HqTransferDetail entity — one allocation line of an HQ transfer (تفاصيل الحوالة,
/// UC-TRF-08, §22.S.3). The header's sum is shared across its lines: Σ line Amount must stay
/// within the header's AmountOfPayment (enforced in the service layer).
/// مصير الحوالة is a nullable bool, not an enum/lookup: the legacy screen's two-option
/// dropdown (لم ينفذ / تم التنفيذ) with null = unset. Execution/arrival data is legal only
/// once the line is marked executed.
/// </summary>
public class HqTransferDetail : FullAuditedEntity
{
    // ========== Parent ==========

    /// <summary>
    /// Foreign key to the owning HqTransfer (required)
    /// </summary>
    public Guid FK_HqTransferId { get; set; }

    // ========== Line fields (§22.S.3) ==========

    /// <summary>
    /// رقم الحوالة — the line's own transfer reference (required)
    /// </summary>
    public string TransferNumber { get; set; } = string.Empty;

    /// <summary>
    /// مبلغ الحوالة — this line's share of the header sum (required, positive)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// التاريخ المتوقع للتحويل (optional)
    /// </summary>
    public DateTime? EstimatedTransferDate { get; set; }

    /// <summary>
    /// مصير الحوالة — false = لم ينفذ, true = تم التنفيذ, null = unset
    /// </summary>
    public bool? IsExecuted { get; set; }

    /// <summary>
    /// تاريخ التنفيذ — legal only when IsExecuted is true
    /// </summary>
    public DateTime? ExecutionDate { get; set; }

    /// <summary>
    /// تاريخ وصول الحوالة — legal only when IsExecuted is true
    /// </summary>
    public DateTime? ArrivalDate { get; set; }

    /// <summary>
    /// مبلغ الوصول — legal only when IsExecuted is true
    /// </summary>
    public decimal? ArrivalAmount { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to the owning transfer
    /// </summary>
    public virtual HqTransfer? HqTransfer { get; set; }
}
