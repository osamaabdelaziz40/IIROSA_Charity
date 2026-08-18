using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.OrphanReport;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Orphan Reports Controller
/// Implements UC-6.1 through UC-6.10 for Orphan Summary Reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrphanReportsController : ControllerBase
{
    private readonly IOrphanReportService _orphanReportService;
    private readonly ILogger<OrphanReportsController> _logger;

    public OrphanReportsController(
        IOrphanReportService orphanReportService,
        ILogger<OrphanReportsController> logger)
    {
        _orphanReportService = orphanReportService;
        _logger = logger;
    }

    #region Report Generation (UC-6.1)

    /// <summary>
    /// Generate orphan report with filters - UC-6.1
    /// Charity users see only their orphans
    /// Admin/Super Admin see all orphans with optional charity filter
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanReportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrphanReportResultDto>> GenerateReport([FromBody] OrphanReportFilterDto filter)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate date range - UC-6.2
            if (filter.FromDate > filter.ToDate)
                return BadRequest(new { message = "From date must be before or equal to To date" });

            var report = await _orphanReportService.GenerateReportAsync(filter);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating orphan report");
            return StatusCode(500, new { message = "Error generating report", error = ex.Message });
        }
    }

    #endregion

    #region Export Operations (UC-6.7)

    /// <summary>
    /// Export orphan report to Excel or PDF - UC-6.7
    /// </summary>
    [HttpPost("export")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportReport([FromBody] OrphanReportFilterDto filter, [FromQuery] OrphanReportExportDto? exportOptions = null)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate date range
            if (filter.FromDate > filter.ToDate)
                return BadRequest(new { message = "From date must be before or equal to To date" });

            exportOptions ??= new OrphanReportExportDto();
            var content = await _orphanReportService.ExportReportAsync(filter, exportOptions);

            var fileName = string.IsNullOrWhiteSpace(exportOptions.FileName)
                ? $"OrphanReport_{filter.FromDate:yyyyMMdd}_{filter.ToDate:yyyyMMdd}.{exportOptions.ExportFormat.ToLower()}"
                : exportOptions.FileName;

            var contentType = exportOptions.ExportFormat.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orphan report");
            return StatusCode(500, new { message = "Error exporting report", error = ex.Message });
        }
    }

    #endregion

    #region Report History (UC-6.9)

    /// <summary>
    /// View report history - UC-6.9
    /// Charity users see only their reports
    /// Admin/Super Admin see all reports
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReportHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _orphanReportService.GetReportHistoryAsync(pageNumber, pageSize);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphan report history");
            return StatusCode(500, new { message = "Error retrieving history", error = ex.Message });
        }
    }

    /// <summary>
    /// Get report history entry by ID
    /// </summary>
    [HttpGet("history/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanReportHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanReportHistoryDto>> GetReportHistoryEntry(Guid id)
    {
        try
        {
            var entry = await _orphanReportService.GetReportHistoryEntryAsync(id);
            if (entry == null)
                return NotFound(new { message = $"Report history entry with ID '{id}' not found" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphan report history entry: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving history entry", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete report from history
    /// </summary>
    [HttpDelete("history/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReportHistoryEntry(Guid id)
    {
        try
        {
            await _orphanReportService.DeleteReportHistoryEntryAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting orphan report history entry: {Id}", id);
            return StatusCode(500, new { message = "Error deleting history entry", error = ex.Message });
        }
    }

    #endregion

    #region Period Comparison (UC-6.10)

    /// <summary>
    /// Compare two report periods - UC-6.10
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("compare")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrphanReportComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrphanReportComparisonDto>> ComparePeriods([FromBody] ComparePeriodsDto dto)
    {
        try
        {
            if (dto.Report1Id == dto.Report2Id)
                return BadRequest(new { message = "Report IDs must be different" });

            var comparison = await _orphanReportService.ComparePeriodsAsync(dto.Report1Id, dto.Report2Id);
            return Ok(comparison);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing orphan report periods");
            return StatusCode(500, new { message = "Error comparing periods", error = ex.Message });
        }
    }

    #endregion

    #region Scheduling (UC-6.8)

    /// <summary>
    /// Schedule recurring report - UC-6.8
    /// </summary>
    [HttpPost("schedule")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> ScheduleRecurringReport([FromBody] ScheduleRecurringReportDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var scheduleId = await _orphanReportService.ScheduleRecurringReportAsync(dto);
            return CreatedAtAction(nameof(GetScheduledReports), new { id = scheduleId }, scheduleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling recurring orphan report");
            return StatusCode(500, new { message = "Error scheduling report", error = ex.Message });
        }
    }

    /// <summary>
    /// Get scheduled reports
    /// </summary>
    [HttpGet("scheduled")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetScheduledReports([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _orphanReportService.GetScheduledReportsAsync(pageNumber, pageSize);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled orphan reports");
            return StatusCode(500, new { message = "Error retrieving scheduled reports", error = ex.Message });
        }
    }

    /// <summary>
    /// Update scheduled report
    /// </summary>
    [HttpPut("scheduled/{scheduleId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateScheduledReport(Guid scheduleId, [FromBody] ScheduleRecurringReportDto dto)
    {
        try
        {
            await _orphanReportService.UpdateScheduledReportAsync(scheduleId, dto);
            return Ok(new { message = "Scheduled report updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scheduled orphan report: {ScheduleId}", scheduleId);
            return StatusCode(500, new { message = "Error updating scheduled report", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete scheduled report
    /// </summary>
    [HttpDelete("scheduled/{scheduleId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteScheduledReport(Guid scheduleId)
    {
        try
        {
            await _orphanReportService.DeleteScheduledReportAsync(scheduleId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting scheduled orphan report: {ScheduleId}", scheduleId);
            return StatusCode(500, new { message = "Error deleting scheduled report", error = ex.Message });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get orphan statistics for dashboard
    /// </summary>
    [HttpPost("statistics")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrphanStatisticsDto>> GetOrphanStatistics([FromBody] OrphanReportFilterDto filter)
    {
        try
        {
            var statistics = await _orphanReportService.GetOrphanStatisticsAsync(filter);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphan statistics");
            return StatusCode(500, new { message = "Error retrieving statistics", error = ex.Message });
        }
    }

    #endregion
}

/// <summary>
/// DTO for comparing periods
/// </summary>
public class ComparePeriodsDto
{
    public Guid Report1Id { get; set; }
    public Guid Report2Id { get; set; }
}
