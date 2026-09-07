using System.Security.Claims;
using IIROSA.Application.DTOs.IncomingOutgoing;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Incoming &amp; Outgoing Correspondence API Controller (epic 16, UC-COR-01…19).
/// Only Admin and Super Admin reach it; delete use cases are the General Director's
/// alone — SuperAdmin only. Paged reads return the { items, totalCount, page } envelope.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class IncomingOutgoingController : ControllerBase
{
    private readonly IIncomingService _incomingService;
    private readonly IOutgoingService _outgoingService;
    private readonly ILogger<IncomingOutgoingController> _logger;

    public IncomingOutgoingController(
        IIncomingService incomingService,
        IOutgoingService outgoingService,
        ILogger<IncomingOutgoingController> logger)
    {
        _incomingService = incomingService;
        _outgoingService = outgoingService;
        _logger = logger;
    }

    // ========== Incoming letters (UC-COR-01…09) ==========

    /// <summary>
    /// The §21.S.1 incoming register (UC-COR-01 / UC-COR-02) — scoped server-side to the
    /// caller's charity.
    /// </summary>
    [HttpGet("incoming")]
    public async Task<IActionResult> GetIncomingLetters([FromQuery] IncomingFilterDto filter)
    {
        try
        {
            var result = await _incomingService.GetPagedAsync(filter ?? new IncomingFilterDto());
            return Ok(new { items = result.Items, totalCount = result.TotalCount, page = result.Page });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving incoming letters");
            return StatusCode(500, new { message = "An error occurred while retrieving incoming letters" });
        }
    }

    /// <summary>View an incoming letter (UC-COR-05).</summary>
    [HttpGet("incoming/{id:guid}")]
    public async Task<IActionResult> GetIncomingLetter(Guid id)
    {
        try
        {
            var letter = await _incomingService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound(new { message = "Incoming letter not found" });
            }

            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving incoming letter {IncomingId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving incoming letter" });
        }
    }

    /// <summary>Register an incoming letter (UC-COR-04 / §21.S.2) — serial allocated server-side.</summary>
    [HttpPost("incoming")]
    public async Task<IActionResult> CreateIncomingLetter([FromBody] CreateIncomingDto model)
    {
        try
        {
            var letter = await _incomingService.CreateAsync(model);
            return CreatedAtAction(nameof(GetIncomingLetter), new { id = letter.Id }, letter);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without
        // this it is swallowed into a 500, leaving the client no `errors` map to flag fields.
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating incoming letter");
            return StatusCode(500, new { message = "An error occurred while creating incoming letter" });
        }
    }

    /// <summary>Update an incoming letter (UC-COR-06) — serial and charity ownership immutable.</summary>
    [HttpPut("incoming/{id:guid}")]
    public async Task<IActionResult> UpdateIncomingLetter(Guid id, [FromBody] UpdateIncomingDto model)
    {
        try
        {
            var letter = await _incomingService.UpdateAsync(id, model);
            return Ok(letter);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without
        // this it is swallowed into a 500, leaving the client no `errors` map to flag fields.
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating incoming letter {IncomingId}", id);
            return StatusCode(500, new { message = "An error occurred while updating incoming letter" });
        }
    }

    /// <summary>Delete an incoming letter (UC-COR-07) — the General Director's alone (SuperAdmin only).</summary>
    [HttpDelete("incoming/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteIncomingLetter(Guid id)
    {
        try
        {
            await _incomingService.DeleteAsync(id, GetCurrentUserId());
            _logger.LogInformation("Incoming letter {IncomingId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Incoming letter deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting incoming letter {IncomingId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting incoming letter" });
        }
    }

    /// <summary>The advisory next incoming serial (UC-COR-03) — read-only form field.</summary>
    [HttpGet("incoming/next-serial")]
    public async Task<IActionResult> GetNextIncomingSerial([FromQuery] int? year, [FromQuery] Guid? charityId)
    {
        try
        {
            var next = await _incomingService.GetNextSerialAsync(year, charityId);
            return Ok(next);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving the next incoming serial");
            return StatusCode(500, new { message = "An error occurred while retrieving the next incoming serial" });
        }
    }

    /// <summary>The §21.S.1 status options — the spec's tri-state, Arabic-stored.</summary>
    [HttpGet("incoming/statuses")]
    public async Task<IActionResult> GetIncomingStatuses()
    {
        try
        {
            var statuses = await _incomingService.GetAvailableStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving incoming letter statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving incoming letter statuses" });
        }
    }

    /// <summary>The two §21.S.3 lists (UC-COR-09): attached employees and available ones.</summary>
    [HttpGet("incoming/{id:guid}/employees")]
    public async Task<IActionResult> GetIncomingEmployees(Guid id)
    {
        try
        {
            var employees = await _incomingService.GetEmployeesAsync(id);
            return Ok(employees);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees of incoming letter {IncomingId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the letter's employees" });
        }
    }

    /// <summary>Attach an employee to an incoming letter (UC-COR-09).</summary>
    [HttpPost("incoming/{id:guid}/employees")]
    public async Task<IActionResult> AttachIncomingEmployee(Guid id, [FromBody] AttachEmployeeDto model)
    {
        try
        {
            await _incomingService.AttachEmployeeAsync(id, model.UserId);
            return Ok(new { message = "Employee attached successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attaching employee {UserId} to incoming letter {IncomingId}", model.UserId, id);
            return StatusCode(500, new { message = "An error occurred while attaching the employee" });
        }
    }

    /// <summary>Detach an employee from an incoming letter (UC-COR-09).</summary>
    [HttpDelete("incoming/{id:guid}/employees/{userId:guid}")]
    public async Task<IActionResult> DetachIncomingEmployee(Guid id, Guid userId)
    {
        try
        {
            await _incomingService.DetachEmployeeAsync(id, userId);
            return Ok(new { message = "Employee detached successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while detaching employee {UserId} from incoming letter {IncomingId}", userId, id);
            return StatusCode(500, new { message = "An error occurred while detaching the employee" });
        }
    }

    // ========== Outgoing letters (UC-COR-10…19) ==========

    /// <summary>
    /// The §21.S.4 outgoing register (UC-COR-10 / UC-COR-11) — scoped server-side to the
    /// caller's charity.
    /// </summary>
    [HttpGet("outgoing")]
    public async Task<IActionResult> GetOutgoingLetters([FromQuery] OutgoingFilterDto filter)
    {
        try
        {
            var result = await _outgoingService.GetPagedAsync(filter ?? new OutgoingFilterDto());
            return Ok(new { items = result.Items, totalCount = result.TotalCount, page = result.Page });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving outgoing letters");
            return StatusCode(500, new { message = "An error occurred while retrieving outgoing letters" });
        }
    }

    /// <summary>View an outgoing letter (UC-COR-14).</summary>
    [HttpGet("outgoing/{id:guid}")]
    public async Task<IActionResult> GetOutgoingLetter(Guid id)
    {
        try
        {
            var letter = await _outgoingService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound(new { message = "Outgoing letter not found" });
            }

            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving outgoing letter {OutgoingId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving outgoing letter" });
        }
    }

    /// <summary>Register an outgoing letter (UC-COR-13 / §21.S.5) — serial allocated server-side.</summary>
    [HttpPost("outgoing")]
    public async Task<IActionResult> CreateOutgoingLetter([FromBody] CreateOutgoingDto model)
    {
        try
        {
            var letter = await _outgoingService.CreateAsync(model);
            return CreatedAtAction(nameof(GetOutgoingLetter), new { id = letter.Id }, letter);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without
        // this it is swallowed into a 500, leaving the client no `errors` map to flag fields.
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating outgoing letter");
            return StatusCode(500, new { message = "An error occurred while creating outgoing letter" });
        }
    }

    /// <summary>Update an outgoing letter (UC-COR-15) — serial and charity ownership immutable.</summary>
    [HttpPut("outgoing/{id:guid}")]
    public async Task<IActionResult> UpdateOutgoingLetter(Guid id, [FromBody] UpdateOutgoingDto model)
    {
        try
        {
            var letter = await _outgoingService.UpdateAsync(id, model);
            return Ok(letter);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without
        // this it is swallowed into a 500, leaving the client no `errors` map to flag fields.
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating outgoing letter {OutgoingId}", id);
            return StatusCode(500, new { message = "An error occurred while updating outgoing letter" });
        }
    }

    /// <summary>Delete an outgoing letter (UC-COR-16) — the General Director's alone (SuperAdmin only).</summary>
    [HttpDelete("outgoing/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteOutgoingLetter(Guid id)
    {
        try
        {
            await _outgoingService.DeleteAsync(id, GetCurrentUserId());
            _logger.LogInformation("Outgoing letter {OutgoingId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Outgoing letter deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting outgoing letter {OutgoingId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting outgoing letter" });
        }
    }

    /// <summary>The advisory next outgoing serial (UC-COR-12) — read-only form field.</summary>
    [HttpGet("outgoing/next-serial")]
    public async Task<IActionResult> GetNextOutgoingSerial([FromQuery] int? year, [FromQuery] Guid? charityId)
    {
        try
        {
            var next = await _outgoingService.GetNextSerialAsync(year, charityId);
            return Ok(next);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving the next outgoing serial");
            return StatusCode(500, new { message = "An error occurred while retrieving the next outgoing serial" });
        }
    }

    /// <summary>The §21.S.5 category options (UC-COR-17) — table-backed, bilingual.</summary>
    [HttpGet("outgoing/categories")]
    public async Task<IActionResult> GetOutgoingCategories()
    {
        try
        {
            var categories = await _outgoingService.GetAvailableCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving outgoing categories");
            return StatusCode(500, new { message = "An error occurred while retrieving outgoing categories" });
        }
    }

    /// <summary>The two §21.S.6 grids (UC-COR-18): attached orphans and the charity's unattached ones.</summary>
    [HttpGet("outgoing/{id:guid}/orphans")]
    public async Task<IActionResult> GetOutgoingOrphans(Guid id)
    {
        try
        {
            var orphans = await _outgoingService.GetOrphansAsync(id);
            return Ok(orphans);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving orphans of outgoing letter {OutgoingId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the letter's orphans" });
        }
    }

    /// <summary>Attach an orphan report to an outgoing letter (UC-COR-18 — BR-26 / BR-27 guards).</summary>
    [HttpPost("outgoing/{id:guid}/orphans")]
    public async Task<IActionResult> AttachOutgoingOrphan(Guid id, [FromBody] AttachOrphanDto model)
    {
        try
        {
            await _outgoingService.AttachOrphanAsync(id, model.OrphanId);
            return Ok(new { message = "Orphan report attached successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attaching orphan {OrphanId} to outgoing letter {OutgoingId}", model.OrphanId, id);
            return StatusCode(500, new { message = "An error occurred while attaching the orphan report" });
        }
    }

    /// <summary>Detach an orphan report from an outgoing letter (UC-COR-18).</summary>
    [HttpDelete("outgoing/{id:guid}/orphans/{orphanId:guid}")]
    public async Task<IActionResult> DetachOutgoingOrphan(Guid id, Guid orphanId)
    {
        try
        {
            await _outgoingService.DetachOrphanAsync(id, orphanId);
            return Ok(new { message = "Orphan report detached successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while detaching orphan {OrphanId} from outgoing letter {OutgoingId}", orphanId, id);
            return StatusCode(500, new { message = "An error occurred while detaching the orphan report" });
        }
    }

    /// <summary>
    /// The §21.S.7 report (UC-COR-19): outgoing letters carrying the requested orphan's
    /// report. The child code (كود اليتيم) is mandatory.
    /// </summary>
    [HttpGet("outgoing/reports/by-orphans")]
    public async Task<IActionResult> GetOrphansByOutgoingLetter([FromQuery] OutgoingOrphanReportFilterDto filter)
    {
        try
        {
            var result = await _outgoingService.GetOrphanReportAsync(filter ?? new OutgoingOrphanReportFilterDto());
            return Ok(new { items = result.Items, totalCount = result.TotalCount, page = result.Page });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while building the orphans-by-outgoing-letter report");
            return StatusCode(500, new { message = "An error occurred while building the report" });
        }
    }

    /// <summary>The caller's user id from the token — the delete audit stamp.</summary>
    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
