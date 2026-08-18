using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Create OrphanPayment DTO - Used for creating new payment groups (UC-5.1)
/// </summary>
public class CreateOrphanPaymentDto
{
    // Basic Information
    /// <summary>
    /// Group Name (e.g., "January 2026 Payments - Region A")
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
    /// Payment period start date (required)
    /// </summary>
    [Required(ErrorMessage = "Payment period start date is required")]
    public DateTime PaymentPeriodFrom { get; set; }

    /// <summary>
    /// Payment period end date (required)
    /// </summary>
    [Required(ErrorMessage = "Payment period end date is required")]
    public DateTime PaymentPeriodTo { get; set; }

    /// <summary>
    /// Group creation date (defaults to today)
    /// </summary>
    public DateTime? GroupDate { get; set; }

    // Financial Information
    /// <summary>
    /// Exchange rate for reporting purposes (e.g., 0.21 for SAR to EGP)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Exchange rate must be positive")]
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Currency type (EGP, SAR, USD, etc.)
    /// </summary>
    [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
    public string? Currency { get; set; }

    /// <summary>
    /// Locks exchange rate to prevent changes (DontRemoveRate flag)
    /// </summary>
    public bool DontRemoveRate { get; set; } = false;

    // Batch Information
    /// <summary>
    /// Batch number (optional - auto-generated if not provided)
    /// </summary>
    [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
    public string? BatchNo { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int ShowOrder { get; set; }

    // Notes
    /// <summary>
    /// Additional notes about the payment group
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Filtering Options (for UI pre-selection, not stored in entity)
    /// <summary>
    /// Charity filter for orphan selection
    /// </summary>
    public int? CharityId { get; set; }

    /// <summary>
    /// Region filter for orphan selection
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Center filter for orphan selection
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Sponsorship status filter (Sponsored, Unsponsored, All)
    /// </summary>
    public string? SponsorshipStatus { get; set; }

    /// <summary>
    /// Age range filter - from
    /// </summary>
    public int? AgeFrom { get; set; }

    /// <summary>
    /// Age range filter - to
    /// </summary>
    public int? AgeTo { get; set; }
}
