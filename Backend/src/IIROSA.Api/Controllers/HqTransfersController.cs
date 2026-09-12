using IIROSA.Application.DTOs.HqTransfers;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// HQ Financial Transfers API Controller (UC-TRF-01…08): list, view, create, update, per-country
/// ceiling and transfer detail lines.
///
/// Roles: the spec's actors (Fin. Director, Gen. Director) map to the platform's Admin/SuperAdmin
/// — an HQ module; the caller's country claim scopes reads server-side (never menu-only).
/// </summary>
[Authorize(Roles = "Admin,SuperAdmin")]
public class HqTransfersController : ApiController
{
    private readonly IHqTransferService _transferService;

    public HqTransfersController(
        IHqTransferService transferService,
        ILogger<HqTransfersController> logger) : base(logger)
    {
        _transferService = transferService;
    }

    // ========== Reads ==========

    /// <summary>
    /// Get HQ transfers with pagination (UC-TRF-01: list), scoped to the caller's country claim
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HqTransferPagedResult<HqTransferListDto>>> GetHqTransfers(
        [FromQuery] HqTransferFilterDto filter)
    {
        try
        {
            var result = await _transferService.GetHqTransfersAsync(filter);
            return Ok(result);
        }
        catch (Exception)
        {
            // The service layer already logs at Error and rethrows — no duplicate here
            return StatusCode(500, new { message = "An error occurred while retrieving HQ transfers" });
        }
    }

    /// <summary>
    /// Register statistics for the band above the §22.S.1 grid — the list read's country
    /// scope (the caller's country claim, pinned in the service), never the active search.
    /// Literal route beside {id:guid} — literals outrank parameters (export precedent).
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<HqTransferStatisticsDto>> GetStatistics()
    {
        try
        {
            var statistics = await _transferService.GetStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception)
        {
            // The service layer already logs at Error and rethrows — no duplicate here
            return StatusCode(500, new { message = "An error occurred while retrieving HQ transfer statistics" });
        }
    }

    /// <summary>
    /// Export the §22.S.1 register to Excel — the list read's scope rules (the caller's
    /// country claim, pinned in the service), every matching row: paging is ignored.
    /// Literal route beside {id:guid} — literals outrank parameters (max-amount precedent).
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportHqTransfers([FromQuery] HqTransferFilterDto filter)
    {
        try
        {
            var excelBytes = await _transferService.ExportHqTransfersToExcelAsync(filter);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"hq-transfers_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        catch (Exception)
        {
            // The service layer already logs at Error and rethrows — no duplicate here
            return StatusCode(500, new { message = "An error occurred while exporting HQ transfers" });
        }
    }

    /// <summary>
    /// Get one HQ transfer with resolved lookup names (UC-TRF-03: view). The {id:guid}
    /// constraint keeps the literal sibling routes (max-amount, 17-6/17-7) unambiguous.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HqTransferDetailDto>> GetHqTransferById(Guid id)
    {
        try
        {
            var transfer = await _transferService.GetHqTransferByIdAsync(id);
            return Ok(transfer);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving HQ transfer" });
        }
    }

    /// <summary>
    /// The per-country transfer ceiling (UC-TRF-06). Literal route beside {id:guid} — literals
    /// outrank parameters and the guid constraint keeps them unambiguous. Missing countryId →
    /// 400 { message } (controlled shape, not the model-binding problem details).
    /// </summary>
    [HttpGet("max-amount")]
    public async Task<ActionResult<CountryMaxTransferAmountDto>> GetMaxTransferAmount([FromQuery] int? countryId)
    {
        if (!countryId.HasValue)
        {
            return BadRequest(new { message = "countryId is required" });
        }

        try
        {
            var ceiling = await _transferService.GetMaxTransferAmountAsync(countryId.Value);
            return Ok(ceiling);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the transfer ceiling" });
        }
    }

    /// <summary>
    /// The §22.S.3 details screen read (UC-TRF-08) — header summary + allocation lines
    /// </summary>
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<HqTransferDetailsResultDto>> GetTransferDetails(Guid id)
    {
        try
        {
            var details = await _transferService.GetTransferDetailsAsync(id);
            return Ok(details);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the transfer details" });
        }
    }

    // ========== Writes ==========

    /// <summary>
    /// Save one allocation line (UC-TRF-08) — per-row «حفظ». The «Failed Operation» state
    /// gating arrives as the 400 errors map; the sum rule as 400 { message }.
    /// </summary>
    [HttpPut("{id:guid}/details")]
    public async Task<ActionResult<HqTransferDetailLineDto>> SaveTransferDetailLine(
        Guid id,
        [FromBody] SaveHqTransferDetailLineDto model)
    {
        try
        {
            var line = await _transferService.SaveTransferDetailLineAsync(id, model);
            return Ok(line);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while saving the transfer detail line" });
        }
    }


    /// <summary>
    /// Set a country's transfer ceiling (UC-TRF-07) — SuperAdmin ONLY, tighter than the
    /// controller default (the 13-5 SuperAdmin-only-delete precedent): the read stays open to
    /// Admin so 17-6's form guard works for every transfer writer, but setting the limit is a
    /// consequential, few-should-do-it write.
    /// </summary>
    [HttpPut("max-amount")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<CountryMaxTransferAmountDto>> UpdateMaxTransferAmount(
        [FromBody] UpdateCountryMaxTransferDto model)
    {
        try
        {
            var ceiling = await _transferService.UpdateCountryMaxTransferAmountAsync(model);
            return Ok(ceiling);
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
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while setting the transfer ceiling" });
        }
    }

    /// <summary>
    /// Create a new HQ transfer (UC-TRF-02)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HqTransferDetailDto>> AddNewTransfer([FromBody] CreateHqTransferDto model)
    {
        try
        {
            var transfer = await _transferService.AddNewTransferAsync(model);
            return CreatedAtAction(nameof(GetHqTransferById), new { id = transfer.Id }, transfer);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without
        // this it is swallowed into a 500, leaving the client no `errors` map to flag fields
        // against (the OfficeProjectManagementController:89-101 shape, copied verbatim).
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Scope refusal (pin-never-widen, ruled 2026-08-24): the caller's country claim
            // forbids filing under the requested country — 403, nothing written
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating HQ transfer" });
        }
    }

    /// <summary>
    /// Update a transfer (UC-TRF-04) — id in the body (board contract)
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<HqTransferDetailDto>> UpdateHqTransfer([FromBody] UpdateHqTransferDto model)
    {
        try
        {
            var transfer = await _transferService.UpdateHqTransferAsync(model);
            return Ok(transfer);
        }
        // Same ladder as the create action: validation errors BEFORE the catch-all
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Scope refusal (pin-never-widen, ruled 2026-08-24) — 403, nothing written
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating HQ transfer" });
        }
    }
}
