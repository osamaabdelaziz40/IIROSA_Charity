using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.PeriodicOrphanReport;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Periodic Orphan Reports Controller
/// Implements UC-6.11 through UC-6.17 for Periodic Orphan Reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PeriodicOrphanReportsController : ControllerBase
{
    private readonly IPeriodicOrphanReportService _periodicReportService;
    private readonly ILogger<PeriodicOrphanReportsController> _logger;

    public PeriodicOrphanReportsController(
        IPeriodicOrphanReportService periodicReportService,
        ILogger<PeriodicOrphanReportsController> logger)
    {
        _periodicReportService = periodicReportService;
        _logger = logger;
    }

    #region CRUD Operations (UC-6.11)

    /// <summary>
    /// Create new periodic orphan report - UC-6.11
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PeriodicOrphanReportDto>> CreateReport([FromBody] CreatePeriodicOrphanReportDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var report = await _periodicReportService.CreateReportAsync(dto);
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating periodic orphan report");
            return StatusCode(500, new { message = "Error creating periodic orphan report", error = ex.Message });
        }
    }

    /// <summary>
    /// Get report by ID - UC-6.11, UC-6.13, UC-6.14, UC-6.15, UC-6.16
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodicOrphanReportDto>> GetReport(Guid id)
    {
        try
        {
            var report = await _periodicReportService.GetByIdAsync(id);
            if (report == null)
                return NotFound(new { message = $"Report with ID '{id}' not found" });

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving report", error = ex.Message });
        }
    }

    /// <summary>
    /// Update periodic orphan report - UC-6.11
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodicOrphanReportDto>> UpdateReport(Guid id, [FromBody] UpdatePeriodicOrphanReportDto dto)
    {
        try
        {
            dto.Id = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var report = await _periodicReportService.UpdateReportAsync(dto);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error updating report", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete periodic orphan report - UC-6.11
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReport(Guid id)
    {
        try
        {
            await _periodicReportService.DeleteReportAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error deleting report", error = ex.Message });
        }
    }

    #endregion

    #region Review Operations (UC-6.13)

    /// <summary>
    /// Review periodic report (approve or reject) - UC-6.13
    /// Only Super Admin, Admin, Accountant, and Employee can review
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee")]
    [ProducesResponseType(typeof(PeriodicOrphanReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodicOrphanReportDto>> ReviewReport(Guid id, [FromBody] ReviewPeriodicReportDto dto)
    {
        try
        {
            dto.ReportId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var report = await _periodicReportService.ReviewReportAsync(dto);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error reviewing report", error = ex.Message });
        }
    }

    #endregion

    #region List and Filter Operations (UC-6.14, UC-6.15, UC-6.16)

    /// <summary>
    /// Get periodic reports with filtering and pagination - UC-6.14, UC-6.15, UC-6.16
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReports([FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetReportsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan reports");
            return StatusCode(500, new { message = "Error retrieving reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Get approved reports - UC-6.14
    /// </summary>
    [HttpGet("approved")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetApprovedReports([FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetApprovedReportsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approved periodic orphan reports");
            return StatusCode(500, new { message = "Error retrieving approved reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Get rejected reports - UC-6.15
    /// </summary>
    [HttpGet("rejected")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRejectedReports([FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetRejectedReportsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rejected periodic orphan reports");
            return StatusCode(500, new { message = "Error retrieving rejected reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Get reports by orphan - UC-6.16
    /// Returns orphan's complete periodic report history ordered by creation date (newest first)
    /// </summary>
    [HttpGet("by-orphan/{orphanId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReportsByOrphan(Guid orphanId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _periodicReportService.GetReportsByOrphanAsync(orphanId, pageNumber, pageSize);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan reports for orphan: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "Error retrieving orphan reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Get orphan report summary - UC-6.16
    /// </summary>
    [HttpGet("by-orphan/{orphanId}/summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PeriodicOrphanReportSummaryDto>> GetOrphanReportSummary(Guid orphanId)
    {
        try
        {
            var summary = await _periodicReportService.GetOrphanReportSummaryAsync(orphanId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphan report summary: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "Error retrieving summary", error = ex.Message });
        }
    }

    #endregion

    #region Export Operations (UC-6.17)

    /// <summary>
    /// Export periodic reports to Excel - UC-6.17
    /// </summary>
    [HttpPost("export")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromBody] PeriodicOrphanReportFilterDto filter, [FromQuery] bool includeAllFields = false)
    {
        try
        {
            var content = await _periodicReportService.ExportToExcelAsync(filter, includeAllFields);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PeriodicOrphanReports.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting periodic orphan reports to Excel");
            return StatusCode(500, new { message = "Error exporting reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Export orphan's report history to Excel - UC-6.16, UC-6.17
    /// </summary>
    [HttpPost("by-orphan/{orphanId}/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportOrphanHistoryToExcel(Guid orphanId)
    {
        try
        {
            var content = await _periodicReportService.ExportOrphanHistoryToExcelAsync(orphanId);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Orphan_{orphanId}_Reports.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orphan report history to Excel: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "Error exporting history", error = ex.Message });
        }
    }

    #endregion

    #region Status Management (UC-6.11)

    /// <summary>
    /// Lock report - UC-6.11
    /// </summary>
    [HttpPost("{id}/lock")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LockReport(Guid id)
    {
        try
        {
            await _periodicReportService.LockReportAsync(id);
            return Ok(new { message = "Report locked successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error locking report", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlock report - UC-6.11
    /// Only Super Admin and Admin can unlock
    /// </summary>
    [HttpPost("{id}/unlock")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnlockReport(Guid id)
    {
        try
        {
            await _periodicReportService.UnlockReportAsync(id);
            return Ok(new { message = "Report unlocked successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error unlocking report", error = ex.Message });
        }
    }

    /// <summary>
    /// Activate report - UC-6.11
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateReport(Guid id)
    {
        try
        {
            await _periodicReportService.ActivateReportAsync(id);
            return Ok(new { message = "Report activated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error activating report", error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate report - UC-6.11
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateReport(Guid id)
    {
        try
        {
            await _periodicReportService.DeactivateReportAsync(id);
            return Ok(new { message = "Report deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "Error deactivating report", error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if report can be edited
    /// </summary>
    [HttpGet("{id}/can-edit")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanEditReport(Guid id)
    {
        try
        {
            var canEdit = await _periodicReportService.CanEditReportAsync(id);
            return Ok(new { canEdit });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if report can be edited: {Id}", id);
            return StatusCode(500, new { message = "Error checking edit status", error = ex.Message });
        }
    }

    #endregion
}
