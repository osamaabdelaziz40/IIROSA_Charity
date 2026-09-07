using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.OrphanPayment;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// OrphanPayments Controller
/// Implements all orphan payment group endpoints (UC-5.1 through UC-5.13)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrphanPaymentsController : ControllerBase
{
    private readonly IOrphanPaymentService _orphanPaymentService;
    private readonly ILogger<OrphanPaymentsController> _logger;

    public OrphanPaymentsController(
        IOrphanPaymentService orphanPaymentService,
        ILogger<OrphanPaymentsController> logger)
    {
        _orphanPaymentService = orphanPaymentService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all payment groups with filtering and pagination (UC-5.8, UC-5.12, UC-5.13).
    /// UC-ORP-08: with orphanId set this serves one orphan's payment history — the mode charity
    /// callers are limited to (a charity may not list every group).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer,Charity")]
    [ProducesResponseType(typeof(IEnumerable<OrphanPaymentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)>> GetPaymentGroups(
        [FromQuery] OrphanPaymentFilterDto filter)
    {
        try
        {
            // Charity callers: orphan-scoped only, and D4 fail-closed — a Charity token without
            // a parseable charity claim never falls through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && (!filter.OrphanId.HasValue || GetUserCharityId() == null))
            {
                return Forbid();
            }

            var result = await _orphanPaymentService.GetPaymentGroupsAsync(filter, GetUserCharityId(), GetUserRole());
            return Ok(new { result.Items, result.TotalCount });
        }
        // Out-of-scope/nonexistent orphanId is a 404 (P12 no-existence-leak) — the service's
        // scope guard throws KeyNotFoundException; without this clause the generic catch below
        // surfaced it as a 500.
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment groups");
            return StatusCode(500, new { message = "Error retrieving payment groups" });
        }
    }

    /// <summary>
    /// UC-ORP-11 — the distinct batch numbers (رقم الحصة) in the caller's scope, most recent first.
    /// Feeds the orphan payment-details picker and the history dialog's batch filter.
    /// </summary>
    [HttpGet("batch-numbers")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer,Charity")]
    [ProducesResponseType(typeof(IEnumerable<BatchNumberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BatchNumberDto>>> GetBatchNumbers([FromQuery] Guid? charityId)
    {
        try
        {
            // D4 fail-closed (review P6): a Charity token without a parseable charity claim
            // must not fall through to the service as an unscoped (all-batches) caller.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var batchNumbers = await _orphanPaymentService.GetBatchNumbersAsync(GetUserCharityId(), GetUserRole(), charityId);
            return Ok(batchNumbers);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch numbers");
            return StatusCode(500, new { message = "Error retrieving batch numbers" });
        }
    }

    /// <summary>
    /// Get payment group by ID (UC-PAY-03) — HQ-Fin read set per the WAR role matrix.
    /// Charity stays off the batch-header read (orphan-scoped `{id}/details` only, 10-7).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> GetPaymentGroup(Guid id)
    {
        try
        {
            var paymentGroup = await _orphanPaymentService.GetByIdAsync(id);
            if (paymentGroup == null)
            {
                return NotFound(new { message = $"Payment group with ID '{id}' not found" });
            }

            return Ok(paymentGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment group: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving payment group" });
        }
    }

    /// <summary>
    /// Get payment group details with orphans (UC-5.9 / UC-PAY-07).
    /// UC-ORP-09: with orphanId set, the orphan's rows within the batch (batch header stays
    /// complete) — byte-identical single-orphan mode. Without it: HQ gets every row (optionally
    /// narrowed by charityId), a Charity caller gets the shared header with rows filtered to its
    /// own orphans (§15.U.7 — zero rows is an empty list, not a refusal).
    /// </summary>
    [HttpGet("{id}/details")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer,Charity")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> GetPaymentGroupDetails(Guid id, [FromQuery] Guid? orphanId, [FromQuery] Guid? charityId)
    {
        try
        {
            // D4 fail-closed (see GetPaymentGroups): a Charity caller without a charity claim
            // can never be scoped — refuse rather than leak the unfiltered batch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            // charityId is an HQ-only narrowing filter; the claim always pins for Charity callers.
            var effectiveCharityId = User.IsInRole("Charity") ? null : charityId;

            var details = await _orphanPaymentService.GetPaymentGroupDetailsAsync(id, orphanId, GetUserCharityId(), GetUserRole(), effectiveCharityId);
            return Ok(details);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment group details: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving payment group details" });
        }
    }

    /// <summary>
    /// Create new payment group (UC-5.1)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrphanPaymentDto>> CreatePaymentGroup([FromBody] CreateOrphanPaymentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paymentGroup = await _orphanPaymentService.CreatePaymentGroupAsync(dto);
            return CreatedAtAction(nameof(GetPaymentGroup), new { id = paymentGroup.Id }, paymentGroup);
        }
        // Must precede the catch-all: ValidationException derives from Exception (§15.S.2 field map)
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
            _logger.LogError(ex, "Error creating payment group");
            return StatusCode(500, new { message = "Error creating payment group" });
        }
    }

    /// <summary>
    /// Update payment group (UC-5.5 / UC-PAY-04) — header-only write, HQ-Fin role set.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> UpdatePaymentGroup(Guid id, [FromBody] UpdateOrphanPaymentDto dto)
    {
        try
        {
            dto.Id = id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paymentGroup = await _orphanPaymentService.UpdatePaymentGroupAsync(dto);
            return Ok(paymentGroup);
        }
        // Must precede the catch-all: ValidationException derives from Exception (§15.S.2 field map)
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
            _logger.LogError(ex, "Error updating payment group: {Id}", id);
            return StatusCode(500, new { message = "Error updating payment group" });
        }
    }

    /// <summary>
    /// Delete payment group (UC-PAY-05) — soft delete; refused once any row is disbursed
    /// (§25.6 A1) or a periodic report references the batch. HQ-Fin role set.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePaymentGroup(Guid id)
    {
        try
        {
            await _orphanPaymentService.DeletePaymentGroupAsync(id);
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
            _logger.LogError(ex, "Error deleting payment group: {Id}", id);
            return StatusCode(500, new { message = "Error deleting payment group" });
        }
    }

    #endregion

    #region Exchange Rate Management (UC-5.2, UC-5.6)

    /// <summary>
    /// Set exchange rate (UC-5.2) — HQ-Fin role set (10-4)
    /// </summary>
    [HttpPut("{id}/exchange-rate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetExchangeRate(Guid id, [FromBody] SetExchangeRateDto dto)
    {
        try
        {
            dto.OrphanPaymentId = id;
            await _orphanPaymentService.SetExchangeRateAsync(dto);
            return Ok(new { message = "Exchange rate set successfully" });
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
            _logger.LogError(ex, "Error setting exchange rate for payment group: {Id}", id);
            return StatusCode(500, new { message = "Error setting exchange rate" });
        }
    }

    /// <summary>
    /// Lock/Unlock exchange rate (UC-5.6) — typed body (10-4); HQ-Fin role set
    /// </summary>
    [HttpPost("{id}/lock-exchange-rate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LockExchangeRate(Guid id, [FromBody] LockExchangeRateDto dto)
    {
        try
        {
            // Review P25: the lock direction is mandatory — an absent LockRate refuses
            // instead of defaulting to false (a silent UNlock).
            if (dto.LockRate == null)
            {
                return BadRequest(new { message = "lockRate is required" });
            }

            await _orphanPaymentService.LockExchangeRateAsync(id, dto.LockRate.Value);
            return Ok(new { message = $"Exchange rate {(dto.LockRate.Value ? "locked" : "unlocked")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking exchange rate for payment group: {Id}", id);
            return StatusCode(500, new { message = "Error locking exchange rate" });
        }
    }

    #endregion

    #region Orphan Management (UC-5.3, UC-5.4)

    /// <summary>
    /// Get available orphans for adding to payment group (UC-5.3)
    /// </summary>
    [HttpGet("{id}/available-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(typeof(IEnumerable<OrphanForPaymentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<OrphanForPaymentListDto> Items, int TotalCount)>> GetAvailableOrphans(
        Guid id,
        [FromQuery] OrphanFilterForPaymentDto filter)
    {
        try
        {
            filter.OrphanPaymentId = id;
            var result = await _orphanPaymentService.GetAvailableOrphansAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available orphans for payment group: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving available orphans" });
        }
    }

    /// <summary>
    /// Add orphans to payment group (UC-5.3)
    /// </summary>
    [HttpPost("{id}/orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddOrphansToGroup(Guid id, [FromBody] AddOrphansToGroupDto dto)
    {
        try
        {
            dto.OrphanPaymentId = id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (addedCount, skippedCount) = await _orphanPaymentService.AddOrphansToGroupAsync(dto);
            return Ok(new { message = $"Added {addedCount} orphans to group", addedCount, skippedCount });
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
            _logger.LogError(ex, "Error adding orphans to payment group: {Id}", id);
            return StatusCode(500, new { message = "Error adding orphans to payment group" });
        }
    }

    /// <summary>
    /// Remove orphan from payment group (UC-5.4)
    /// </summary>
    [HttpDelete("orphan-items/{orphanPaymentItemId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveOrphanFromGroup(Guid orphanPaymentItemId)
    {
        try
        {
            await _orphanPaymentService.RemoveOrphanFromGroupAsync(orphanPaymentItemId);
            return Ok(new { message = "Orphan removed from group successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing orphan from group: {OrphanPaymentItemId}", orphanPaymentItemId);
            return StatusCode(500, new { message = "Error removing orphan from group" });
        }
    }

    #endregion

    #region Row Actions (UC-PAY-09..13, §15.1 — ONE endpoint)

    /// <summary>
    /// Row action on a payment item (§15.1 action model). 10-9 ships action 0 (stop/resume);
    /// 10-10..10-13 add actions 1..4 on this same endpoint. Charity callers act on their own
    /// rows only (item→orphan→charity scope), and may not resume an HQ-initiated stop (BR-16/21).
    /// </summary>
    [HttpPost("orphan-items")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer,Charity")]
    [ProducesResponseType(typeof(OrphanPaymentItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentItemDto>> UpdateOrphanPaymentItem([FromBody] UpdateOrphanPaymentItemDto dto)
    {
        try
        {
            // D4 fail-closed (review P1): a Charity token without a parseable charity claim
            // never reaches the service as an unscoped caller — belt and braces with the
            // service's own fail-closed scope check.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);
            var item = await _orphanPaymentService.UpdateOrphanPaymentItemAsync(
                dto, GetUserCharityId(), GetUserRole(), userId == Guid.Empty ? null : userId);
            return Ok(item);
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
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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
            _logger.LogError(ex, "Error applying row action to orphan payment item: {Id}", dto.OrphanPaymentItemId);
            return StatusCode(500, new { message = "Error applying the payment row action"});
        }
    }

    #endregion

    #region Batch Number Management (UC-5.11)

    /// <summary>
    /// Assign batch number (UC-5.11)
    /// </summary>
    [HttpPut("{id}/batch-number")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignBatchNumber(Guid id, [FromBody] AssignBatchNumberDto dto)
    {
        try
        {
            dto.OrphanPaymentId = id;
            await _orphanPaymentService.AssignBatchNumberAsync(dto);
            return Ok(new { message = "Batch number assigned successfully" });
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
            _logger.LogError(ex, "Error assigning batch number for payment group: {Id}", id);
            return StatusCode(500, new { message = "Error assigning batch number" });
        }
    }

    #endregion

    #region Upload Status Management (UC-5.7)

    /// <summary>
    /// Mark group as uploaded/unuploaded (UC-5.7)
    /// </summary>
    [HttpPost("{id}/mark-uploaded")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsUploaded(Guid id, [FromBody] MarkAsUploadedDto dto)
    {
        try
        {
            dto.OrphanPaymentId = id;
            await _orphanPaymentService.MarkAsUploadedAsync(dto);
            return Ok(new { message = $"Payment group {(dto.IsUploaded ? "marked as uploaded" : "unmarked")}" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment group as uploaded: {Id}", id);
            return StatusCode(500, new { message = "Error marking payment group as uploaded" });
        }
    }

    #endregion

    #region Export (UC-5.10)

    /// <summary>
    /// Export payment group report (UC-5.10)
    /// </summary>
    [HttpGet("{id}/export")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ExportPaymentGroup(
        Guid id,
        [FromQuery] string format = "Excel",
        [FromQuery] bool includePhotos = false,
        [FromQuery] string groupBy = "None")
    {
        try
        {
            var content = await _orphanPaymentService.ExportPaymentGroupAsync(id, format, includePhotos, groupBy);

            if (content.Length == 0)
            {
                return BadRequest(new { message = "Export functionality not yet implemented" });
            }

            string contentType = format.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/vnd.ms-excel"
            };

            string fileName = $"PaymentGroup_{id}_{DateTime.UtcNow:yyyyMMdd}.{format.ToLowerInvariant()}";

            return File(content, contentType, fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payment group: {Id}", id);
            return StatusCode(500, new { message = "Error exporting payment group" });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get payment group by batch number
    /// </summary>
    [HttpGet("by-batch-no/{batchNo}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer,Charity")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> GetByBatchNo(string batchNo)
    {
        try
        {
            // D4 fail-closed (review P6): a Charity token without a parseable charity claim
            // must not resolve batch headers unscoped.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            // 10-6: the charity claim scopes the resolve — participation is checked in the
            // service, and a batch outside the charity's enrolment reads as 404.
            var paymentGroup = await _orphanPaymentService.GetByBatchNoAsync(batchNo, GetUserCharityId(), GetUserRole());
            if (paymentGroup == null)
            {
                return NotFound(new { message = $"Payment group with batch number '{batchNo}' not found" });
            }

            return Ok(paymentGroup);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment group by batch no: {BatchNo}", batchNo);
            return StatusCode(500, new { message = "Error retrieving payment group" });
        }
    }

    #endregion

    #region Caller Scope Helpers

    /// <summary>
    /// The caller's charity id from the <see cref="IiroSaClaimTypes.CharityId"/> claim — the same
    /// scoping source FamiliesController uses for the orphan-scoped reads (UC-ORP-08/09/11).
    /// </summary>
    private Guid? GetUserCharityId()
    {
        var charityIdClaim = User.FindFirst(IiroSaClaimTypes.CharityId)?.Value;
        if (Guid.TryParse(charityIdClaim, out var charityId))
        {
            return charityId;
        }
        return null;
    }

    private string? GetUserRole()
    {
        // D4: tenancy keys on the Charity role wherever it appears — not on which role claim
        // happens to be listed first. A multi-role user holding Charity is charity-scoped.
        if (User.IsInRole("Charity"))
        {
            return "Charity";
        }
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    }

    #endregion
}
