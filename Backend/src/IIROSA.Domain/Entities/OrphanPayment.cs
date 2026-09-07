using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// OrphanPayment entity - Represents a payment group/batch for orphan payments
/// Implements UC-5.1 through UC-5.13
/// Inherits from FullAuditedEntity which provides CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted
/// Note: This is a GROUP/BATCH of orphans for manual/offline payment processing, NOT actual payment processing
/// </summary>
public class OrphanPayment : FullAuditedEntity
{
    // Basic Information
    /// <summary>
    /// Group/Batch Name (e.g., "January 2026 Payments - Region A")
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the payment group
    /// </summary>
    public string? Description { get; set; }

    // Payment Period
    /// <summary>
    /// Payment period start date (required)
    /// </summary>
    public DateTime PaymentPeriodFrom { get; set; }

    /// <summary>
    /// Payment period end date (required)
    /// </summary>
    public DateTime PaymentPeriodTo { get; set; }

    /// <summary>
    /// Group creation date (defaults to today)
    /// </summary>
    public DateTime GroupDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Distribution start date تاريخ بدء التوزيع (§15.S.2 mandatory on create; nullable for legacy rows)
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    // Financial Information
    /// <summary>
    /// Exchange rate for reporting purposes (e.g., 0.21 for SAR to EGP)
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Currency type (EGP, SAR, USD, etc.)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Locks exchange rate to prevent changes (DontRemoveRate flag)
    /// </summary>
    public bool DontRemoveRate { get; set; } = false;

    // Batch Information
    /// <summary>
    /// Batch number for tracking and reference (auto-generated or manual)
    /// </summary>
    public string? BatchNo { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int ShowOrder { get; set; }

    // Status
    /// <summary>
    /// Indicates if the group has been marked as uploaded/ready for manual processing
    /// </summary>
    public bool IsBatchUploaded { get; set; } = false;

    /// <summary>
    /// Upload timestamp when marked as uploaded
    /// </summary>
    public DateTime? UploadDate { get; set; }

    // Notes
    /// <summary>
    /// Additional notes about the payment group
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    /// <summary>
    /// Collection of orphans in this payment group
    /// </summary>
    public virtual ICollection<OrphanPaymentItem> Orphans { get; set; } = new List<OrphanPaymentItem>();
}
