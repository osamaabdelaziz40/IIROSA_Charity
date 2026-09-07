using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// UC-RPT-32 (§23.U.32 صفحة ملخص الصرف) — the payment batch summary figures served to
/// the batch view's totals band. Read-only: composes over <c>TableNoTracking</c> and
/// never writes.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// The summary figures of the payment batch that <paramref name="paymentId"/>'s group
    /// belongs to (groups sharing its BatchNo aggregate together — the 18-29/30/31 batch
    /// resolution). Charity scope follows the 18-22/24 ladder server-side: a charity claim
    /// pins, an HQ <paramref name="charityId"/> narrows, a country claim intersects; the
    /// scope never widens. An empty batch returns zeroed figures plus a message — never
    /// an exception (AC 4).
    /// </summary>
    /// <param name="paymentId">The payment group id the batch view is showing.</param>
    /// <param name="charityId">HQ-only narrow; ignored when the caller carries a charity claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PaymentSummaryDto> GetPaymentSummaryAsync(
        Guid paymentId,
        Guid? charityId = null,
        CancellationToken cancellationToken = default);
}
