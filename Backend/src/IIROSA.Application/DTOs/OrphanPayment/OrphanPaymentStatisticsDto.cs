namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Register statistics band shown above the payment-groups grid (UC-5.8, §15.S.1).
/// Batch-level by design — the register lists groups, not orphan rows. Counts follow the
/// caller's scope exactly like GetPaymentGroupsAsync: for a Charity-role caller, batches
/// with at least one live row tenanted to their charity; head-office callers see all.
/// </summary>
public class OrphanPaymentStatisticsDto
{
    /// <summary>Live payment groups (batches) in the caller's scope.</summary>
    public int Total { get; set; }
    /// <summary>Batches marked as uploaded (IsBatchUploaded, UC-5.7).</summary>
    public int Uploaded { get; set; }
    /// <summary>Batches not yet uploaded — Total minus Uploaded, so the pair always sums to Total.</summary>
    public int PendingUpload { get; set; }
    /// <summary>Batches registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
