using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.IncomingOutgoing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IIROSA.Domain.Entities;

namespace IIROSA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

    #region Incoming Letters

    /// <summary>
    /// Get incoming letter by ID
    /// </summary>
    [HttpGet("incoming/{id}")]
    public async Task<ActionResult<IncomingDto>> GetIncomingById(Guid id)
    {
        try
        {
            var result = await _incomingService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get paginated list of incoming letters
    /// </summary>
    [HttpGet("incoming")]
    public async Task<ActionResult<(IEnumerable<IncomingListDto> Items, int TotalCount)>> GetIncomingPaged([FromQuery] IncomingFilterDto filter)
    {
        var result = await _incomingService.GetPagedAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Create new incoming letter
    /// </summary>
    [HttpPost("incoming")]
    public async Task<ActionResult<IncomingDto>> CreateIncoming([FromBody] CreateIncomingDto dto)
    {
        try
        {
            var result = await _incomingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetIncomingById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update incoming letter
    /// </summary>
    [HttpPut("incoming/{id}")]
    public async Task<ActionResult<IncomingDto>> UpdateIncoming(Guid id, [FromBody] UpdateIncomingDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var result = await _incomingService.UpdateAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete incoming letter
    /// </summary>
    [HttpDelete("incoming/{id}")]
    public async Task<ActionResult> DeleteIncoming(Guid id)
    {
        try
        {
            await _incomingService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion

    #region Outgoing Letters

    /// <summary>
    /// Get outgoing letter by ID
    /// </summary>
    [HttpGet("outgoing/{id}")]
    public async Task<ActionResult<OutgoingDto>> GetOutgoingById(Guid id)
    {
        try
        {
            var result = await _outgoingService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get paginated list of outgoing letters
    /// </summary>
    [HttpGet("outgoing")]
    public async Task<ActionResult<(IEnumerable<OutgoingListDto> Items, int TotalCount)>> GetOutgoingPaged([FromQuery] OutgoingFilterDto filter)
    {
        var result = await _outgoingService.GetPagedAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Create new outgoing letter
    /// </summary>
    [HttpPost("outgoing")]
    public async Task<ActionResult<OutgoingDto>> CreateOutgoing([FromBody] CreateOutgoingDto dto)
    {
        try
        {
            var result = await _outgoingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetOutgoingById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update outgoing letter
    /// </summary>
    [HttpPut("outgoing/{id}")]
    public async Task<ActionResult<OutgoingDto>> UpdateOutgoing(Guid id, [FromBody] UpdateOutgoingDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var result = await _outgoingService.UpdateAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete outgoing letter
    /// </summary>
    [HttpDelete("outgoing/{id}")]
    public async Task<ActionResult> DeleteOutgoing(Guid id)
    {
        try
        {
            await _outgoingService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion

    #region Import Operations

    /// <summary>
    /// Validate incoming letters import file (UC-12.3)
    /// </summary>
    [HttpPost("import/incoming/validate")]
    public async Task<ActionResult<ImportValidationResultDto>> ValidateIncomingImport([FromBody] ImportIncomingRequestDto request)
    {
        try
        {
            var result = await _incomingService.ValidateImportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Import incoming letters (UC-12.1)
    /// </summary>
    [HttpPost("import/incoming")]
    public async Task<ActionResult<ImportResultDto>> ImportIncoming([FromBody] ImportIncomingRequestDto request)
    {
        try
        {
            var result = await _incomingService.ImportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Validate outgoing letters import file (UC-12.3)
    /// </summary>
    [HttpPost("import/outgoing/validate")]
    public async Task<ActionResult<ImportValidationResultDto>> ValidateOutgoingImport([FromBody] ImportOutgoingRequestDto request)
    {
        try
        {
            var result = await _outgoingService.ValidateImportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Import outgoing letters (UC-12.2)
    /// </summary>
    [HttpPost("import/outgoing")]
    public async Task<ActionResult<ImportResultDto>> ImportOutgoing([FromBody] ImportOutgoingRequestDto request)
    {
        try
        {
            var result = await _outgoingService.ImportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Download import template (UC-12.9)
    /// </summary>
    [HttpGet("import/template/{type}")]
    public async Task<ActionResult> DownloadTemplate(string type)
    {
        try
        {
            TemplateDownloadDto template;

            if (type.ToLower() == "incoming")
            {
                template = await _incomingService.DownloadTemplateAsync();
            }
            else if (type.ToLower() == "outgoing")
            {
                template = await _outgoingService.DownloadTemplateAsync();
            }
            else
            {
                return BadRequest("Invalid template type. Use 'incoming' or 'outgoing'");
            }

            return File(template.FileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", template.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Export Operations

    /// <summary>
    /// Export incoming letters (UC-12.10)
    /// </summary>
    [HttpPost("export/incoming")]
    public async Task<ActionResult<ExportResultDto>> ExportIncoming([FromBody] ExportIncomingRequestDto request)
    {
        try
        {
            var result = await _incomingService.ExportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Export outgoing letters (UC-12.11)
    /// </summary>
    [HttpPost("export/outgoing")]
    public async Task<ActionResult<ExportResultDto>> ExportOutgoing([FromBody] ExportOutgoingRequestDto request)
    {
        try
        {
            var result = await _outgoingService.ExportAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Import/Export History

    /// <summary>
    /// Get import history (UC-12.7)
    /// </summary>
    [HttpGet("import/history")]
    public async Task<ActionResult<IEnumerable<ImportHistoryItemDto>>> GetImportHistory()
    {
        var incomingHistory = await _incomingService.GetImportHistoryAsync();
        var outgoingHistory = await _outgoingService.GetImportHistoryAsync();

        var allHistory = incomingHistory.Concat(outgoingHistory);
        return Ok(allHistory);
    }

    /// <summary>
    /// Rollback import (UC-12.8)
    /// </summary>
    [HttpPost("import/rollback/{importId}")]
    public async Task<ActionResult> RollbackImport(Guid importId)
    {
        try
        {
            await _incomingService.RollbackImportAsync(importId);
            return Ok(new { message = "Import rolled back successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get export history (UC-12.14)
    /// </summary>
    [HttpGet("export/history")]
    public async Task<ActionResult<IEnumerable<ExportHistoryItemDto>>> GetExportHistory()
    {
        var history = await _outgoingService.GetExportHistoryAsync();
        return Ok(history);
    }

    #endregion

    #region Business Logic Endpoints

    /// <summary>
    /// Check if Incoming ID is unique
    /// </summary>
    [HttpGet("incoming/check-unique/{incomingId}")]
    public async Task<ActionResult<bool>> CheckIncomingIdUnique(string incomingId, Guid? excludeId = null)
    {
        var result = await _incomingService.IsIncomingIdUniqueAsync(incomingId, excludeId);
        return Ok(result);
    }

    /// <summary>
    /// Check if Outgoing ID is unique
    /// </summary>
    [HttpGet("outgoing/check-unique/{outgoingId}")]
    public async Task<ActionResult<bool>> CheckOutgoingIdUnique(string outgoingId, Guid? excludeId = null)
    {
        var result = await _outgoingService.IsOutgoingIdUniqueAsync(outgoingId, excludeId);
        return Ok(result);
    }

    /// <summary>
    /// Get next serial number for incoming letters
    /// </summary>
    [HttpGet("incoming/next-serial")]
    public async Task<ActionResult<int>> GetNextIncomingSerial([FromQuery] int? departmentId, [FromQuery] int? year)
    {
        var result = await _incomingService.GetNextSerialNumberAsync(departmentId, year);
        return Ok(result);
    }

    /// <summary>
    /// Get next serial number for outgoing letters
    /// </summary>
    [HttpGet("outgoing/next-serial")]
    public async Task<ActionResult<int>> GetNextOutgoingSerial([FromQuery] int? departmentId, [FromQuery] int? year)
    {
        var result = await _outgoingService.GetNextSerialNumberAsync(departmentId, year);
        return Ok(result);
    }

    /// <summary>
    /// Get available statuses for incoming letters
    /// </summary>
    [HttpGet("incoming/statuses")]
    public async Task<ActionResult<IEnumerable<string>>> GetIncomingStatuses()
    {
        var result = await _incomingService.GetAvailableStatusesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get status colors for incoming letters
    /// </summary>
    [HttpGet("incoming/status-colors")]
    public async Task<ActionResult<Dictionary<string, string>>> GetIncomingStatusColors()
    {
        var result = await _incomingService.GetStatusColorsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get available categories for outgoing letters
    /// </summary>
    [HttpGet("outgoing/categories")]
    public async Task<ActionResult<Dictionary<int, string>>> GetOutgoingCategories()
    {
        var result = await _outgoingService.GetAvailableCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create child outgoing letter
    /// </summary>
    [HttpPost("outgoing/{parentOutgoingId}/child")]
    public async Task<ActionResult<OutgoingDto>> CreateChildOutgoing(Guid parentOutgoingId, [FromBody] CreateChildOutgoingDto dto)
    {
        try
        {
            var result = await _outgoingService.CreateChildOutgoingAsync(parentOutgoingId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get child outgoing letters
    /// </summary>
    [HttpGet("outgoing/{parentOutgoingId}/children")]
    public async Task<ActionResult<IEnumerable<ChildOutGoingDto>>> GetChildOutgoings(Guid parentOutgoingId)
    {
        try
        {
            var result = await _outgoingService.GetChildOutgoingsAsync(parentOutgoingId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete child outgoing letter
    /// </summary>
    [HttpDelete("outgoing/child/{childId}")]
    public async Task<ActionResult> DeleteChildOutgoing(Guid childId)
    {
        try
        {
            var result = await _outgoingService.DeleteChildOutgoingAsync(childId);
            if (result)
                return Ok(new { message = "Child outgoing letter deleted successfully" });
            else
                return BadRequest("Failed to delete child outgoing letter");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion
}
