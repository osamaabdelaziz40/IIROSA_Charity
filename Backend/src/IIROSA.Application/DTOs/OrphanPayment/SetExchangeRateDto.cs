using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Set Exchange Rate DTO - Used for setting exchange rate (UC-5.2, UC-5.6)
/// </summary>
public class SetExchangeRateDto
{
    /// <summary>
    /// Payment group ID
    /// </summary>
    [Required(ErrorMessage = "Payment group ID is required")]
    public Guid OrphanPaymentId { get; set; }

    /// <summary>
    /// Exchange rate (e.g., 0.21 for SAR to EGP)
    /// </summary>
    [Required(ErrorMessage = "Exchange rate is required")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Exchange rate must be positive")]
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Currency type (EGP, SAR, USD, etc.)
    /// </summary>
    [Required(ErrorMessage = "Currency is required")]
    [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Lock exchange rate to prevent changes (DontRemoveRate flag) (UC-5.6)
    /// </summary>
    public bool DontRemoveRate { get; set; } = false;
}
