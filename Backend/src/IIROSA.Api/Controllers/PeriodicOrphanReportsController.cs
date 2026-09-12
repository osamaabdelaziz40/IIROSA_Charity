using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IIROSA.Application.DTOs.PeriodicOrphanReport;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Periodic Orphan Reports Controller — epic 9 (UC-ORR-01 … UC-ORR-17, from WAR UC-6.11–6.17).
/// Authorisation is enforced per endpoint (roles); data scoping to the caller's
/// charity/country is enforced in the service layer, never here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PeriodicOrphanReportsController : ApiController
{
    private readonly IPeriodicOrphanReportService _periodicReportService;
    private readonly ILogger<PeriodicOrphanReportsController> _logger;

    public PeriodicOrphanReportsController(
        IPeriodicOrphanReportService periodicReportService,
        ILogger<ApiController> baseLogger,
        ILogger<PeriodicOrphanReportsController> logger)
        : base(baseLogger)
    {
        _periodicReportService = periodicReportService;
        _logger = logger;
    }

    #region CRUD Operations (UC-ORR-03 … UC-ORR-06)

    /// <summary>
    /// Create new periodic orphan report - UC-ORR-03
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PeriodicOrphanReportDto>> CreateReport([FromBody] CreatePeriodicOrphanReportDto dto)
    {
        try
        {
            var report = await _periodicReportService.CreateReportAsync(dto);
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating periodic orphan report");
            return StatusCode(500, new { message = "An error occurred while creating the periodic report" });
        }
    }

    /// <summary>
    /// Get report by ID - UC-ORR-04 (view a periodic report)
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
            return StatusCode(500, new { message = "An error occurred while retrieving the report" });
        }
    }

    /// <summary>
    /// Update periodic orphan report - UC-ORR-05
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
            var report = await _periodicReportService.UpdateReportAsync(dto);
            return Ok(report);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the report" });
        }
    }

    /// <summary>
    /// Delete periodic orphan report (soft delete) - UC-ORR-06
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReport(Guid id)
    {
        try
        {
            await _periodicReportService.DeleteReportAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the report" });
        }
    }

    #endregion

    #region Orphan Lookup (UC-ORR-02)

    /// <summary>
    /// Look up an orphan by sponsorship code - UC-ORR-02.
    /// Returns the orphan identity plus report counters, or 404 when the code is
    /// unknown or outside the caller's scope.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(OrphanLookupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanLookupDto>> GetOrphanByCode(string code)
    {
        try
        {
            var orphan = await _periodicReportService.GetOrphanByCodeAsync(code);
            if (orphan == null)
                return NotFound(new { message = $"No orphan found with code '{code}'" });

            return Ok(orphan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up orphan by code: {Code}", code);
            return StatusCode(500, new { message = "An error occurred while looking up the orphan" });
        }
    }

    #endregion

    #region Review Operations (UC-ORR-07, UC-ORR-08)

    /// <summary>
    /// Review periodic report (accept or refuse) - UC-ORR-07 / UC-ORR-08
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
            var report = await _periodicReportService.ReviewReportAsync(dto);
            return Ok(report);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            // Review D4 2026-08-26: out-of-scope or non-HQ review attempts are Forbid, not 500
            // (also covers GetScopedReportAsync's tenancy refusal).
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while reviewing the report" });
        }
    }

    #endregion

    #region List and Filter Operations (UC-ORR-01, UC-ORR-09, UC-ORR-12, UC-ORR-13)

    /// <summary>
    /// Get periodic reports with filtering and pagination - UC-ORR-01 / UC-ORR-09
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>>> GetReports(
        [FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetReportsAsync(filter);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan reports");
            return StatusCode(500, new { message = "An error occurred while retrieving reports" });
        }
    }

    /// <summary>
    /// Register statistics for the band above the periodic reports grid (§14.S.1, UC-ORR-01) —
    /// same caller scope and roles as the register read above: tenancy is applied by the
    /// service from the token (charity pin or country pin).
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PeriodicOrphanReportStatisticsDto>> GetStatistics()
    {
        try
        {
            return Ok(await _periodicReportService.GetStatisticsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan report statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving report statistics" });
        }
    }

    /// <summary>
    /// Get accepted reports - UC-ORR-12
    /// </summary>
    [HttpGet("approved")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>>> GetApprovedReports(
        [FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetApprovedReportsAsync(filter);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accepted periodic orphan reports");
            return StatusCode(500, new { message = "An error occurred while retrieving accepted reports" });
        }
    }

    /// <summary>
    /// Get refused reports - UC-ORR-13
    /// </summary>
    [HttpGet("rejected")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>>> GetRejectedReports(
        [FromQuery] PeriodicOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _periodicReportService.GetRejectedReportsAsync(filter);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refused periodic orphan reports");
            return StatusCode(500, new { message = "An error occurred while retrieving refused reports" });
        }
    }

    /// <summary>
    /// Get reports by orphan - UC-ORR-01 (+ UC-HOU-06 §11.S.3 through the childOrParent param)
    /// Returns the beneficiary's complete periodic report history, newest first. Without the
    /// discriminator param this is the epic-9 orphan read, byte-identical. childOrParent=Child
    /// keeps the strict child filter; childOrParent=Parent resolves the housing family's
    /// guardian (beneficiaryId = provider id) and lists the family's guardian reports.
    /// </summary>
    [HttpGet("by-orphan/{orphanId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>>> GetReportsByOrphan(
        Guid orphanId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? childOrParent = null)
    {
        try
        {
            // §11.S.3 discriminator: absent → epic-9 behaviour unchanged; Child/Parent → the
            // housing read (strict ChildOrParent filter, guardian branch for Parent).
            if (!string.IsNullOrWhiteSpace(childOrParent))
            {
                if (string.Equals(childOrParent, "Child", StringComparison.OrdinalIgnoreCase))
                {
                    var childResult = await _periodicReportService.GetHousingBeneficiaryReportsAsync(
                        orphanId, IIROSA.Domain.Enums.ReportBeneficiaryType.Child, pageNumber, pageSize);
                    return Ok(childResult);
                }

                if (string.Equals(childOrParent, "Parent", StringComparison.OrdinalIgnoreCase))
                {
                    var parentResult = await _periodicReportService.GetHousingBeneficiaryReportsAsync(
                        orphanId, IIROSA.Domain.Enums.ReportBeneficiaryType.Parent, pageNumber, pageSize);
                    return Ok(parentResult);
                }

                return BadRequest(new
                {
                    message = "Invalid childOrParent value",
                    errors = new { childOrParent = new[] { "childOrParent must be 'Child' or 'Parent'" } }
                });
            }

            var result = await _periodicReportService.GetReportsByOrphanAsync(orphanId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodic orphan reports for orphan: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "An error occurred while retrieving the orphan's reports" });
        }
    }

    /// <summary>
    /// Get orphan report counters - UC-ORR-01
    /// </summary>
    [HttpGet("by-orphan/{orphanId}/summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(PeriodicOrphanReportSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodicOrphanReportSummaryDto>> GetOrphanReportSummary(Guid orphanId)
    {
        try
        {
            var summary = await _periodicReportService.GetOrphanReportSummaryAsync(orphanId);
            return Ok(summary);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphan report summary: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "An error occurred while retrieving the summary" });
        }
    }

    #endregion

    #region Export Operations (UC-ORR-11)

    /// <summary>
    /// Export periodic reports to Excel - UC-ORR-11
    /// </summary>
    [HttpPost("export")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel(
        [FromBody] PeriodicOrphanReportFilterDto filter, [FromQuery] bool includeAllFields = false)
    {
        try
        {
            var content = await _periodicReportService.ExportToExcelAsync(filter, includeAllFields);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PeriodicOrphanReports.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting periodic orphan reports to Excel");
            return StatusCode(500, new { message = "An error occurred while exporting reports" });
        }
    }

    /// <summary>
    /// Export orphan's report history to Excel - UC-ORR-01 / UC-ORR-11
    /// </summary>
    [HttpPost("by-orphan/{orphanId}/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportOrphanHistoryToExcel(Guid orphanId)
    {
        try
        {
            var content = await _periodicReportService.ExportOrphanHistoryToExcelAsync(orphanId);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Orphan_{orphanId}_Reports.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orphan report history to Excel: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "An error occurred while exporting the history" });
        }
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Lock report (prevent modifications)
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
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while locking the report" });
        }
    }

    /// <summary>
    /// Unlock report - Only Super Admin and Admin can unlock
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
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while unlocking the report" });
        }
    }

    /// <summary>
    /// Activate report
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
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while activating the report" });
        }
    }

    /// <summary>
    /// Deactivate report
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
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating periodic orphan report: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating the report" });
        }
    }

    #endregion

    #region Helper Endpoints

    /// <summary>
    /// Check if report can be edited (not locked, not accepted)
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
            return StatusCode(500, new { message = "An error occurred while checking edit status" });
        }
    }

    /// <summary>
    /// Check if the current user can review reports - UC-ORR-07 / UC-ORR-08 gate
    /// </summary>
    [HttpGet("can-review")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanReviewReports()
    {
        try
        {
            var userId = CurrentUserId is { Length: > 0 } && Guid.TryParse(CurrentUserId, out var parsed)
                ? parsed
                : Guid.Empty;
            var canReview = await _periodicReportService.CanUserReviewReportsAsync(userId);
            return Ok(new { canReview });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking review permission");
            return StatusCode(500, new { message = "An error occurred while checking review permission" });
        }
    }

    #endregion
}
