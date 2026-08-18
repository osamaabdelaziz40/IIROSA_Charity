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
