namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Lock/unlock a payment group's exchange rate (UC-5.6) — typed body for
/// POST {id}/lock-exchange-rate, replacing the raw bool binding (10-4 defect 4).
/// </summary>
public class LockExchangeRateDto
{
    /// <summary>
    /// true locks the rate (DontRemoveRate), false unlocks it. Review P25: nullable — a
    /// body without the field must refuse (400) instead of silently posting an UNlock.
    /// </summary>
    public bool? LockRate { get; set; }
}
