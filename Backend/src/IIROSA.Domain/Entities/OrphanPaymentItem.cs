using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// OrphanPaymentItem entity - Represents an orphan within a payment group
/// This is the mapping/join entity between Orphan and OrphanPayment
/// </summary>
public class OrphanPaymentItem : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to the OrphanPayment (payment group)
    /// </summary>
    public Guid OrphanPaymentId { get; set; }

    /// <summary>
    /// Foreign key to the Orphan
    /// </summary>
    public Guid OrphanId { get; set; }

    /// <summary>
    /// Display order within the payment group
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Any specific notes for this orphan in the payment group
    /// </summary>
    public string? Notes { get; set; }

    // §15.1 row ledger — whole-epic column set (EP-10, migration Epic10_PaymentDisbursement).
    // Frozen §15.1 action numbering on POST /api/OrphanPayments/orphan-items (review P20 —
    // keep these doc refs aligned with UpdateOrphanPaymentItemDto):
    // 0 = stop/resume (Flag carries the direction) · 1 = printed · 2 = receipt
    // 3 = cheque record · 4 = clear
    /// <summary>
    /// Amount snapshotted from the orphan's current monthly amount at enrolment (BR-17)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Row stopped (§15.1 action 0, Flag=true) — excluded from disbursement; HQ or the
    /// owning charity may stop, see StoppedByUserId for the HQ-stop lock
    /// </summary>
    public bool IsStopped { get; set; } = false;

    /// <summary>
    /// When the row was stopped
    /// </summary>
    public DateTime? StoppedOn { get; set; }

    /// <summary>
    /// Row marked printed (§15.1 action 1)
    /// </summary>
    public bool IsPrinted { get; set; } = false;

    /// <summary>
    /// When the row was marked printed
    /// </summary>
    public DateTime? PrintedOn { get; set; }

    /// <summary>
    /// Receipt confirmed by the charity (§15.1 action 2)
    /// </summary>
    public bool IsGotIt { get; set; } = false;

    /// <summary>
    /// When receipt was confirmed
    /// </summary>
    public DateTime? ReceivedOn { get; set; }

    /// <summary>
    /// Cheque number (§15.1 action 3; action 4 clears it)
    /// </summary>
    public string? ChiqueNum { get; set; }

    /// <summary>
    /// Cheque date (wire name ChiqueDate — §15.1 verbatim)
    /// </summary>
    public DateTime? Printdate { get; set; }

    /// <summary>
    /// Collector name at the charity (§15.1 action 3; action 4 clears it)
    /// </summary>
    public string? BenificiaryName { get; set; }

    /// <summary>
    /// Bank transfer number (bank-file round trip, 10-14..10-17)
    /// </summary>
    public string? TransferNo { get; set; }

    /// <summary>
    /// Exchange status: 0=Pending, 1=Executed, 2=Failed
    /// </summary>
    public int? ExchangeStatus { get; set; }

    /// <summary>
    /// 10-9 (BR-16/BR-21 realisation): who stopped the row — set on stop, cleared on resume.
    /// An HQ stop may not be resumed by a Charity caller (HQ-stop lock). Plain Guid, no FK —
    /// identity users live in another schema (Framework.Identity).
    /// </summary>
    public Guid? StoppedByUserId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Reference to the payment group this item belongs to
    /// </summary>
    public virtual OrphanPayment? OrphanPayment { get; set; }

    /// <summary>
    /// Reference to the orphan
    /// </summary>
    public virtual Orphan? Orphan { get; set; }
}
