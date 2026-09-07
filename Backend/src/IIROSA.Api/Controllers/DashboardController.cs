using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Interfaces;

namespace IIROSA.Api.Controllers;

/// <summary>
/// UC-RPT-32 (§23.U.32 صفحات ملخص الدفعة) — the payment batch summary figures served to the
/// orphan-payments batch view's totals band. Greenfield: the story's reuse clause was checked —
/// EP-10's 10-21 line naming this endpoint has not landed anything, no prior
/// <c>DashboardController</c> exists. Raw envelope + anonymous <c>{ message }</c> errors per the
/// 15-1 ruling; <c>ControllerBase</c> per the 17-1 convention (the story's DoD names it).
/// </summary>
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// The batch's cover figures — orphan count, total disbursed, per-state counts/amounts
    /// (received / not-received / stopped) and cheque totals. HQ-only surface
    /// (<c>SuperAdmin,Admin</c> — AC 5: a Charity-role token is refused 403); the service still
    /// pins a charity caller server-side from the claim (defense in depth: the scope ladder
    /// holds even if the role gate ever widens — AC 3). An empty batch returns zeroed figures
    /// plus a message, never an error (AC 4).
    /// </summary>
    [HttpGet("payment-summary")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(PaymentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentSummary(
        [FromQuery] Guid paymentId,
        [FromQuery] Guid? charityId)
    {
        try
        {
            var summary = await _dashboardService.GetPaymentSummaryAsync(paymentId, charityId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the service ladder's fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment summary failed (UC-RPT-32) for payment {PaymentId}", paymentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while building the payment summary." });
        }
    }
}
