namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Batch number reference row (UC-ORP-11 — orphan reference data): a distinct batch number with
/// the date of its most recent batch, most-recent first. The رقم الحصة picker's data source.
/// </summary>
public class BatchNumberDto
{
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? LatestGroupDate { get; set; }
}
