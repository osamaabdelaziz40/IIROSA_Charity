using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Check Management API Controller
/// Implements UC-11.1 to UC-11.10: Check CRUD operations and specialized actions
/// Follows approved Framework.Core architecture
/// IMPORTANT: Only Admin, Super Admin, and Accountant roles can access this controller.
/// Charity users are explicitly blocked from this module.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin,Accountant")]
public class CheckManagementController : ControllerBase
{
    private readonly ICheckService _checkService;
    private readonly ILogger<CheckManagementController> _logger;

    public CheckManagementController(
        ICheckService checkService,
        ILogger<CheckManagementController> logger)
    {
        _checkService = checkService;
        _logger = logger;
    }

    // ========== CRUD Operations ==========

    /// <summary>
    /// Get all checks with filtering and pagination (UC-11.7: View Checks List)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CheckPagedResult<CheckListDto>>> GetChecks(
        [FromQuery] CheckFilterDto filter)
    {
        try
        {
            var result = await _checkService.GetChecksFilteredAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving checks");
            return StatusCode(500, new { message = "An error occurred while retrieving checks" });
        }
    }

    /// <summary>
    /// Get check by ID (UC-11.8: View Check Details)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CheckDetailDto>> GetCheck(Guid id)
    {
        try
        {
            var check = await _checkService.GetCheckByIdAsync(id);
            if (check == null)
            {
                return NotFound(new { message = "Check not found" });
            }

            return Ok(check);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving check" });
        }
    }

    /// <summary>
    /// Create new check (UC-11.1: Create Check)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CheckDetailDto>> CreateCheck([FromBody] CreateCheckDto model)
    {
        try
        {
            var check = await _checkService.CreateCheckAsync(model);
            return CreatedAtAction(nameof(GetCheck), new { id = check.Id }, check);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating check");
            return StatusCode(500, new { message = "An error occurred while creating check" });
        }
    }

    /// <summary>
    /// Update check
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CheckDetailDto>> UpdateCheck(Guid id, [FromBody] UpdateCheckDto model)
    {
        try
        {
            var check = await _checkService.UpdateCheckAsync(id, model);
            return Ok(check);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while updating check" });
        }
    }

    /// <summary>
    /// Delete check
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCheck(Guid id)
    {
        try
        {
            await _checkService.DeleteCheckAsync(id);
            _logger.LogInformation("Check {CheckId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Check deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting check" });
        }
    }

    // ========== Check-Specific Operations ==========

    /// <summary>
    /// Set check amount (UC-11.3: Set Check Amount)
    /// </summary>
    [HttpPut("{id}/amount")]
    public async Task<ActionResult> SetCheckAmount(Guid id, [FromBody] SetCheckAmountDto model)
    {
        try
        {
            await _checkService.SetCheckAmountAsync(id, model);
            _logger.LogInformation("Check amount updated for check {CheckId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Check amount updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting check amount for check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while setting check amount" });
        }
    }

    /// <summary>
    /// Set check date (UC-11.4: Set Check Date)
    /// </summary>
    [HttpPut("{id}/date")]
    public async Task<ActionResult> SetCheckDate(Guid id, [FromBody] SetCheckDateDto model)
    {
        try
        {
            await _checkService.SetCheckDateAsync(id, model);
            _logger.LogInformation("Check date updated for check {CheckId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Check date updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting check date for check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while setting check date" });
        }
    }

    /// <summary>
    /// Mark check as cleared (UC-11.5: Mark Check as Cleared)
    /// </summary>
    [HttpPut("{id}/clear")]
    public async Task<ActionResult> MarkCheckAsCleared(Guid id, [FromBody] MarkCheckClearedDto model)
    {
        try
        {
            await _checkService.MarkCheckAsClearedAsync(id, model);
            _logger.LogInformation("Check marked as cleared: {CheckId} by {ClearedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Check marked as cleared successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking check as cleared {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while marking check as cleared" });
        }
    }

    /// <summary>
    /// Void check (UC-11.6: Void Check)
    /// </summary>
    [HttpPut("{id}/void")]
    public async Task<ActionResult> VoidCheck(Guid id, [FromBody] VoidCheckDto model)
    {
        try
        {
            await _checkService.VoidCheckAsync(id, model);
            _logger.LogInformation("Check voided: {CheckId} by {VoidedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Check voided successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while voiding check {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while voiding check" });
        }
    }

    /// <summary>
    /// Reconcile checks (UC-11.9: Reconcile Checks)
    /// </summary>
    [HttpPost("reconcile")]
    public async Task<ActionResult<CheckReconciliationDto>> ReconcileChecks([FromBody] CheckReconciliationDto model)
    {
        try
        {
            var result = await _checkService.ReconcileChecksAsync(model);
            _logger.LogInformation("Checks reconciled: {Count} checks by {ReconciledBy}", result.TotalChecksReconciled, User.Identity?.Name);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reconciling checks");
            return StatusCode(500, new { message = "An error occurred while reconciling checks" });
        }
    }

    /// <summary>
    /// Generate check report (UC-11.10: Generate Check Report)
    /// </summary>
    [HttpPost("report")]
    public async Task<ActionResult<CheckReportDto>> GenerateReport([FromBody] CheckReportFilterDto filter)
    {
        try
        {
            var report = await _checkService.GenerateCheckReportAsync(filter);
            _logger.LogInformation("Check report generated by {GeneratedBy}", User.Identity?.Name);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating check report");
            return StatusCode(500, new { message = "An error occurred while generating check report" });
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get pending checks
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<CheckListDto>>> GetPendingChecks()
    {
        try
        {
            var pendingChecks = await _checkService.GetPendingChecksAsync();
            return Ok(pendingChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pending checks");
            return StatusCode(500, new { message = "An error occurred while retrieving pending checks" });
        }
    }

    /// <summary>
    /// Get issued checks
    /// </summary>
    [HttpGet("issued")]
    public async Task<ActionResult<List<CheckListDto>>> GetIssuedChecks()
    {
        try
        {
            var issuedChecks = await _checkService.GetIssuedChecksAsync();
            return Ok(issuedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving issued checks");
            return StatusCode(500, new { message = "An error occurred while retrieving issued checks" });
        }
    }

    /// <summary>
    /// Get cleared checks
    /// </summary>
    [HttpGet("cleared")]
    public async Task<ActionResult<List<CheckListDto>>> GetClearedChecks()
    {
        try
        {
            var clearedChecks = await _checkService.GetClearedChecksAsync();
            return Ok(clearedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cleared checks");
            return StatusCode(500, new { message = "An error occurred while retrieving cleared checks" });
        }
    }

    /// <summary>
    /// Get voided checks
    /// </summary>
    [HttpGet("voided")]
    public async Task<ActionResult<List<CheckListDto>>> GetVoidedChecks()
    {
        try
        {
            var voidedChecks = await _checkService.GetVoidedChecksAsync();
            return Ok(voidedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving voided checks");
            return StatusCode(500, new { message = "An error occurred while retrieving voided checks" });
        }
    }

    /// <summary>
    /// Get unreconciled checks
    /// </summary>
    [HttpGet("unreconciled")]
    public async Task<ActionResult<List<CheckListDto>>> GetUnreconciledChecks()
    {
        try
        {
            var unreconciledChecks = await _checkService.GetUnreconciledChecksAsync();
            return Ok(unreconciledChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unreconciled checks");
            return StatusCode(500, new { message = "An error occurred while retrieving unreconciled checks" });
        }
    }

    /// <summary>
    /// Get check status summary
    /// </summary>
    [HttpGet("status-summary")]
    public async Task<ActionResult<CheckStatusSummaryDto>> GetStatusSummary()
    {
        try
        {
            var summary = await _checkService.GetCheckStatusSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving check status summary");
            return StatusCode(500, new { message = "An error occurred while retrieving check status summary" });
        }
    }

    // ========== Export ==========

    /// <summary>
    /// Export checks to Excel (UC-11.7: Export to Excel)
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportChecks([FromBody] CheckFilterDto filter)
    {
        try
        {
            var excelBytes = await _checkService.ExportChecksToExcelAsync(filter);

            _logger.LogInformation("Checks exported to Excel by {ExportedBy}", User.Identity?.Name);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"checks_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting checks to Excel");
            return StatusCode(500, new { message = "An error occurred while exporting checks to Excel" });
        }
    }
}
