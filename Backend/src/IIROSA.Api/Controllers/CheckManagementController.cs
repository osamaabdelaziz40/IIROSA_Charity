using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// General cheques (chapter 16, UC-CHQ-01..10): the register (§16.S.1), the cheque
/// form with تفقيط (§16.S.2), and the cheque statement (§16.S.3).
///
/// Reads are open to the financial read roles; issue/edit is the financial
/// approver set. Charity users hold no cheque screens, but the service still
/// scopes every row to the caller's charity from the token (defence in depth).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckManagementController : ControllerBase
{
    /// <summary>Register/statement readers — financial read roles.</summary>
    private const string ReadRoles = "SuperAdmin,Admin,Accountant,FinancialOfficer";

    /// <summary>Issue/edit — financial approver roles only.</summary>
    private const string WriteRoles = "SuperAdmin,Accountant,FinancialOfficer";

    private readonly ICheckService _checkService;
    private readonly ILogger<CheckManagementController> _logger;

    public CheckManagementController(
        ICheckService checkService,
        ILogger<CheckManagementController> logger)
    {
        _checkService = checkService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CHQ-01 — the cheque register. Charity, bank, date range, cheque type and
    /// free-text filters; paged. Tenancy is applied by the service from the token.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<CheckPagedResult<CheckListDto>>> GetChecks([FromQuery] CheckFilterDto filter)
    {
        try
        {
            return Ok(await _checkService.GetChecksAsync(filter));
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
            _logger.LogError(ex, "Error retrieving cheques with filter {@Filter}", filter);
            return StatusCode(500, new { message = "An error occurred while retrieving cheques" });
        }
    }

    /// <summary>
    /// The cheque register (§16.S.1 grid) as an Excel workbook — the current filters,
    /// every matching row. Same scope rules as the register read: tenancy is applied
    /// by the service from the token.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> ExportChecks([FromQuery] CheckFilterDto filter)
    {
        try
        {
            var excelBytes = await _checkService.ExportChecksToExcelAsync(filter);

            _logger.LogInformation("Cheque register exported to Excel by {ExportedBy}", User.Identity?.Name);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"general-checks_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
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
            _logger.LogError(ex, "Error exporting cheques to Excel with filter {@Filter}", filter);
            return StatusCode(500, new { message = "An error occurred while exporting cheques" });
        }
    }

    /// <summary>
    /// UC-CHQ-07 — convert an amount into its Arabic words تفقيط for the given currency.
    /// </summary>
    [HttpGet("amount-in-words")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<AmountInWordsDto>> GetAmountInWords(
        [FromQuery] decimal amount,
        [FromQuery] string currency)
    {
        try
        {
            return Ok(await _checkService.GetAmountInWordsAsync(amount, currency));
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid input" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting amount {Amount} {Currency} to words", amount, currency);
            return StatusCode(500, new { message = "An error occurred while converting the amount" });
        }
    }

    /// <summary>
    /// UC-CHQ-09 — the cheque statement بيان الشيكات: the register's filters plus the
    /// per-currency totals, rendered for printing.
    /// </summary>
    [HttpGet("report")]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<CheckStatementDto>> GetStatement([FromQuery] CheckFilterDto filter)
    {
        try
        {
            return Ok(await _checkService.GetStatementAsync(filter));
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
            _logger.LogError(ex, "Error building the cheque statement with filter {@Filter}", filter);
            return StatusCode(500, new { message = "An error occurred while building the statement" });
        }
    }

    /// <summary>
    /// UC-CHQ-03 — a single cheque for review or edit.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<CheckDetailDto>> GetCheck(Guid id)
    {
        try
        {
            var check = await _checkService.GetCheckByIdAsync(id);
            return check is null
                ? NotFound(new { message = "Check not found" })
                : Ok(check);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cheque {CheckId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the cheque" });
        }
    }

    /// <summary>
    /// UC-CHQ-02 — issue a cheque (§16.S.2). Bank, beneficiary name, cheque date,
    /// cheque number, currency and amount are mandatory; the Arabic words are stamped
    /// server-side when the caller does not send them.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<CheckDetailDto>> CreateCheck([FromBody] CreateCheckDto model)
    {
        try
        {
            var check = await _checkService.CreateCheckAsync(model);
            return CreatedAtAction(nameof(GetCheck), new { id = check.Id }, check);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without this
        // it is swallowed into a 500, leaving the client no `errors` map to flag fields against.
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
            _logger.LogError(ex, "Error issuing cheque {@Model}", model);
            return StatusCode(500, new { message = "An error occurred while issuing the cheque" });
        }
    }

    /// <summary>
    /// UC-CHQ-04 — update a cheque. The record id travels in the body, matching the
    /// legacy contract (PUT /api/CheckManagement with { id, ... }).
    /// </summary>
    [HttpPut]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<CheckDetailDto>> UpdateCheck([FromBody] UpdateCheckDto model)
    {
        try
        {
            return Ok(await _checkService.UpdateCheckAsync(model));
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
            _logger.LogError(ex, "Error updating cheque {@Model}", model);
            return StatusCode(500, new { message = "An error occurred while updating the cheque" });
        }
    }
}
