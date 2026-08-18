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
    /// Get all payment groups with filtering and pagination (UC-5.8, UC-5.12, UC-5.13)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrphanPaymentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)>> GetPaymentGroups(
        [FromQuery] OrphanPaymentFilterDto filter)
    {
        try
        {
            var result = await _orphanPaymentService.GetPaymentGroupsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment groups");
            return StatusCode(500, new { message = "Error retrieving payment groups", error = ex.Message });
        }
    }

    /// <summary>
    /// Get payment group by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error retrieving payment group", error = ex.Message });
        }
    }

    /// <summary>
    /// Get payment group details with orphans (UC-5.9)
    /// </summary>
    [HttpGet("{id}/details")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> GetPaymentGroupDetails(Guid id)
    {
        try
        {
            var details = await _orphanPaymentService.GetPaymentGroupDetailsAsync(id);
            return Ok(details);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment group details: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving payment group details", error = ex.Message });
        }
    }

    /// <summary>
    /// Create new payment group (UC-5.1)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment group");
            return StatusCode(500, new { message = "Error creating payment group", error = ex.Message });
        }
    }

    /// <summary>
    /// Update payment group (UC-5.5)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error updating payment group", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete payment group
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment group: {Id}", id);
            return StatusCode(500, new { message = "Error deleting payment group", error = ex.Message });
        }
    }

    #endregion

    #region Exchange Rate Management (UC-5.2, UC-5.6)

    /// <summary>
    /// Set exchange rate (UC-5.2)
    /// </summary>
    [HttpPut("{id}/exchange-rate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error setting exchange rate", error = ex.Message });
        }
    }

    /// <summary>
    /// Lock/Unlock exchange rate (UC-5.6)
    /// </summary>
    [HttpPost("{id}/lock-exchange-rate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LockExchangeRate(Guid id, [FromBody] bool lockRate)
    {
        try
        {
            await _orphanPaymentService.LockExchangeRateAsync(id, lockRate);
            return Ok(new { message = $"Exchange rate {(lockRate ? "locked" : "unlocked")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking exchange rate for payment group: {Id}", id);
            return StatusCode(500, new { message = "Error locking exchange rate", error = ex.Message });
        }
    }

    #endregion

    #region Orphan Management (UC-5.3, UC-5.4)

    /// <summary>
    /// Get available orphans for adding to payment group (UC-5.3)
    /// </summary>
    [HttpGet("{id}/available-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error retrieving available orphans", error = ex.Message });
        }
    }

    /// <summary>
    /// Add orphans to payment group (UC-5.3)
    /// </summary>
    [HttpPost("{id}/orphans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error adding orphans to payment group", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove orphan from payment group (UC-5.4)
    /// </summary>
    [HttpDelete("orphan-items/{orphanPaymentItemId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            return StatusCode(500, new { message = "Error removing orphan from group", error = ex.Message });
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
            return StatusCode(500, new { message = "Error assigning batch number", error = ex.Message });
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
            return StatusCode(500, new { message = "Error marking payment group as uploaded", error = ex.Message });
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
            return StatusCode(500, new { message = "Error exporting payment group", error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get payment group by batch number
    /// </summary>
    [HttpGet("by-batch-no/{batchNo}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrphanPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanPaymentDto>> GetByBatchNo(string batchNo)
    {
        try
        {
            var paymentGroup = await _orphanPaymentService.GetByBatchNoAsync(batchNo);
            if (paymentGroup == null)
            {
                return NotFound(new { message = $"Payment group with batch number '{batchNo}' not found" });
            }

            return Ok(paymentGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment group by batch no: {BatchNo}", batchNo);
            return StatusCode(500, new { message = "Error retrieving payment group", error = ex.Message });
        }
    }

    #endregion
}
