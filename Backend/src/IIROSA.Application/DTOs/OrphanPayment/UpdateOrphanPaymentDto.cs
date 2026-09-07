using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Update OrphanPayment DTO - Used for updating payment groups (UC-5.5)
/// </summary>
public class UpdateOrphanPaymentDto
{
    /// <summary>
    /// Payment group ID
    /// </summary>
    [Required(ErrorMessage = "Payment group ID is required")]
    public Guid Id { get; set; }

    // Basic Information
    /// <summary>
    /// Group Name
    /// </summary>
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the payment group
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    // Payment Period
    /// <summary>
    /// Payment period start date
    /// </summary>
    [Required(ErrorMessage = "Payment period start date is required")]
    public DateTime PaymentPeriodFrom { get; set; }

    /// <summary>
    /// Payment period end date
    /// </summary>
    [Required(ErrorMessage = "Payment period end date is required")]
    public DateTime PaymentPeriodTo { get; set; }

    /// <summary>
    /// Group creation date
    /// </summary>
    public DateTime? GroupDate { get; set; }

    /// <summary>
    /// Distribution start date تاريخ بدء التوزيع (§15.S.2)
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    // Financial Information
    /// <summary>
    /// Exchange rate for reporting purposes
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Exchange rate must be positive")]
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Currency type (EGP, SAR, USD, etc.)
    /// </summary>
    [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
    public string? Currency { get; set; }

    /// <summary>
    /// Locks exchange rate to prevent changes
    /// </summary>
    public bool DontRemoveRate { get; set; } = false;

    // Batch Information
    /// <summary>
    /// Batch number
    /// </summary>
    [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
    public string? BatchNo { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int ShowOrder { get; set; }

    // Status
    /// <summary>
    /// Indicates if the group has been marked as uploaded
    /// </summary>
    public bool IsBatchUploaded { get; set; }

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime? UploadDate { get; set; }

    // Notes
    /// <summary>
    /// Additional notes about the payment group
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}
