using IIROSA.Application.DTOs.OrphanReportCycles;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Orphan Report Cycles API controller (UC-ORC-01…09, docs/Modules/26-UC-ORC).
///
/// Management (register, create, update, delete) is head office only — the spec's
/// Admin/SuperAdmin actors, enforced per action below (never menu-only). The chooser
/// read <c>orphan/{orphanId}/report-status</c> runs on charity screens and is open to
/// every authenticated caller: it returns the live In Progress cycles with the
/// orphan's submission state, and the service scopes nothing away — a cycle is
/// platform-wide and the orphan id is the caller's own screen context. The coverage
/// report lives on ReportsController (POST /api/Reports/cycle-reports) per the
/// reports conventions.
/// </summary>
public class OrphanReportCyclesController : ApiController
{
    private readonly IOrphanReportCycleService _cycleService;

    public OrphanReportCyclesController(
        IOrphanReportCycleService cycleService,
        ILogger<OrphanReportCyclesController> logger) : base(logger)
    {
        _cycleService = cycleService;
    }

    // ========== Management reads (UC-ORC-01) ==========

    /// <summary>
    /// The HQ register — paged, newest first, with report counts and the progress column.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<OrphanReportCyclePagedResult>> GetCycles(
        [FromQuery] OrphanReportCycleFilterDto filter)
    {
        try
        {
            return Ok(await _cycleService.GetFilteredAsync(filter));
        }
        catch (Exception)
        {
            // The service layer already logs at Error and rethrows — no duplicate here
            return StatusCode(500, new { message = "An error occurred while retrieving the report cycles" });
        }
    }

    /// <summary>Detail read for the edit screen (UC-ORC-03).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<OrphanReportCycleDto>> GetCycle(Guid id)
    {
        try
        {
            var cycle = await _cycleService.GetByIdAsync(id);
            return cycle == null
                ? NotFound(new { message = "Report cycle not found" })
                : Ok(cycle);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the report cycle" });
        }
    }

    // ========== Management writes (UC-ORC-02…04) ==========

    /// <summary>
    /// Create a cycle (UC-ORC-02) — status is computed from the dates; a period that
    /// already runs is born In Progress. Overlapping cycles are legal.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<OrphanReportCycleDto>> CreateCycle([FromBody] CreateOrphanReportCycleDto model)
    {
        try
        {
            var result = await _cycleService.CreateAsync(model);
            return CreatedAtAction(nameof(GetCycle), new { id = result.Id }, result);
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
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating the report cycle" });
        }
    }

    /// <summary>
    /// Update a cycle (UC-ORC-03) — completed cycles are read-only; a running cycle's
    /// end date may be extended but never pulled before today (BR-ORC-08).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<OrphanReportCycleDto>> UpdateCycle(Guid id, [FromBody] UpdateOrphanReportCycleDto model)
    {
        try
        {
            model.Id = id;
            return Ok(await _cycleService.UpdateAsync(model));
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
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating the report cycle" });
        }
    }

    /// <summary>
    /// Soft-delete a cycle (UC-ORC-04) — only one that never started and has no bound
    /// reports (BR-ORC-07); completed cycles are the audit record of a reporting round.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteCycle(Guid id)
    {
        try
        {
            await _cycleService.DeleteAsync(id);
            return Ok(new { message = "Report cycle deleted" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the report cycle" });
        }
    }

    // ========== Picker reads (UC-ORC-06/09) ==========

    /// <summary>
    /// Every non-deleted cycle with its live status — the read behind the charity
    /// screens' report-entry button gating and the coverage report's cycle dropdown.
    /// Reference data, no counts: open to every authenticated caller, unlike the HQ
    /// register above.
    /// </summary>
    [HttpGet("selector")]
    public async Task<ActionResult<List<OrphanReportCycleSelectorDto>>> GetSelector()
    {
        try
        {
            return Ok(await _cycleService.GetSelectorAsync());
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the report cycles" });
        }
    }

    // ========== Chooser (UC-ORC-06) ==========

    /// <summary>
    /// The orphan-context chooser payload: every live In Progress cycle with this
    /// orphan's submission state inside it. Refreshes due status transitions first
    /// (the lazy half of UC-ORC-05), so a delayed background tick cannot serve a
    /// stale window. An empty list means the «إضافة تقرير دوري» action stays hidden.
    /// </summary>
    [HttpGet("orphan/{orphanId:guid}/report-status")]
    public async Task<ActionResult<List<OrphanCycleReportStatusDto>>> GetOrphanCycleReportStatus(Guid orphanId)
    {
        try
        {
            return Ok(await _cycleService.GetOrphanCycleReportStatusAsync(orphanId));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while reading the report cycle status" });
        }
    }
}
