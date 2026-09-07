using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.TechnicalSupport;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Support Tickets Controller
/// Implements all Technical Support endpoints (UC-13.1 through UC-13.10)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SupportTicketsController : ControllerBase
{
    private readonly ISupportTicketService _ticketService;
    private readonly ILogger<SupportTicketsController> _logger;

    public SupportTicketsController(
        ISupportTicketService ticketService,
        ILogger<SupportTicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Create new support ticket (UC-13.1)
    /// Available to all authenticated users
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Fill system information only when the client didn't supply it — overwriting
            // unconditionally discarded the form's detected context and the user's answer
            // to "what were you doing?" (UC-CST-01 reproduction context).
            if (string.IsNullOrWhiteSpace(dto.BrowserInfo))
                dto.BrowserInfo = Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(dto.PageUrl))
                dto.PageUrl = Request.Headers.Referer.ToString();
            if (string.IsNullOrWhiteSpace(dto.UserAction))
                dto.UserAction = "Create Ticket";

            var ticket = await _ticketService.CreateTicketAsync(dto, userId);
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => string.Join("; ", g.Select(f => f.ErrorMessage)));
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket");
            return StatusCode(500, new { message = "Error creating support ticket" });
        }
    }

    /// <summary>
    /// Get ticket by ID (UC-13.8: View Ticket Details)
    /// Users can view their own tickets, Admins can view all tickets
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SupportTicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SupportTicketDetailDto>> GetTicket(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var hasAccess = await _ticketService.HasUserAccessAsync(id, userId);
            var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

            if (!hasAccess && !isAdmin)
            {
                return Forbid();
            }

            var ticket = await _ticketService.GetTicketDetailsAsync(id, userId, isAdmin);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ticket: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving ticket" });
        }
    }

    /// <summary>
    /// Update a support ticket (UC-CST-04: Update a support ticket تعديل الطلب)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupportTicketDto>> UpdateTicket(Guid id, [FromBody] UpdateSupportTicketDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var updated = await _ticketService.UpdateTicketAsync(dto, userId);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => string.Join("; ", g.Select(f => f.ErrorMessage)));
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket: {Id}", id);
            return StatusCode(500, new { message = "Error updating ticket" });
        }
    }

    /// <summary>
    /// Get ticket lookups (categories, priorities, statuses) for the ticket form selects.
    /// Available to all authenticated users — the create form needs it too.
    /// </summary>
    [HttpGet("lookups")]
    [ProducesResponseType(typeof(TicketLookupsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketLookupsDto>> GetTicketLookups()
    {
        try
        {
            var lookups = await _ticketService.GetTicketLookupsAsync();
            return Ok(lookups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ticket lookups");
            return StatusCode(500, new { message = "Error retrieving ticket lookups" });
        }
    }

    /// <summary>
    /// Get my tickets (UC-13.3: View My Tickets)
    /// Available to all authenticated users
    /// </summary>
    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(IEnumerable<SupportTicketListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SupportTicketListDto> Items, int TotalCount)>> GetMyTickets(
        [FromQuery] SupportTicketFilterDto filter)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var result = await _ticketService.GetMyTicketsAsync(userId, filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my tickets");
            return StatusCode(500, new { message = "Error retrieving tickets" });
        }
    }

    /// <summary>
    /// Get all tickets (UC-13.4: View All Tickets)
    /// Admin/Super Admin only
    /// </summary>
    [HttpGet("all-tickets")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(IEnumerable<SupportTicketListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SupportTicketListDto> Items, int TotalCount)>> GetAllTickets(
        [FromQuery] SupportTicketFilterDto filter)
    {
        try
        {
            var result = await _ticketService.GetAllTicketsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tickets");
            return StatusCode(500, new { message = "Error retrieving tickets" });
        }
    }

    /// <summary>
    /// Search tickets (UC-13.9: Search Tickets)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("search")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(IEnumerable<SupportTicketListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SupportTicketListDto> Items, int TotalCount)>> SearchTickets(
        [FromBody] SupportTicketFilterDto filter)
    {
        try
        {
            var result = await _ticketService.SearchTicketsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tickets");
            return StatusCode(500, new { message = "Error searching tickets" });
        }
    }

    /// <summary>
    /// Delete ticket
    /// Admin/Super Admin only
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTicket(Guid id)
    {
        try
        {
            var deletedBy = GetCurrentUserId() ?? string.Empty;
            await _ticketService.DeleteTicketAsync(id, deletedBy);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ticket: {Id}", id);
            return StatusCode(500, new { message = "Error deleting ticket" });
        }
    }

    #endregion

    #region Status Management (UC-13.5: Update Ticket Status)

    /// <summary>
    /// Update ticket status (UC-13.5)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)
    {
        try
        {
            if (id != dto.TicketId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var adminUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized(new { message = "Admin user not found" });
            }

            await _ticketService.UpdateTicketStatusAsync(dto, adminUserId);
            return Ok(new { message = "Ticket status updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket status: {Id}", id);
            return StatusCode(500, new { message = "Error updating ticket status" });
        }
    }

    #endregion

    #region Resolution Management (UC-13.6: Mark Ticket as Solved)

    /// <summary>
    /// Mark ticket as solved (UC-13.6)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("{id}/mark-solved")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> MarkTicketAsSolved(Guid id, [FromBody] MarkTicketSolvedDto dto)
    {
        try
        {
            if (id != dto.TicketId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var adminUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized(new { message = "Admin user not found" });
            }

            await _ticketService.MarkTicketAsSolvedAsync(dto, adminUserId);
            return Ok(new { message = "Ticket marked as solved successfully" });
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
            _logger.LogError(ex, "Error marking ticket as solved: {Id}", id);
            return StatusCode(500, new { message = "Error marking ticket as solved" });
        }
    }

    #endregion

    #region Response Management (UC-13.7: Add Ticket Response)

    /// <summary>
    /// Add response to ticket (UC-13.7)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("{id}/responses")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> AddTicketResponse(Guid id, [FromBody] CreateTicketResponseDto dto)
    {
        try
        {
            if (id != dto.TicketId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var responderUserId = GetCurrentUserId();
            var responderName = User.FindFirst("name")?.Value ?? "Admin";
            var responderEmail = User.FindFirst("email")?.Value ?? "admin@iirosa.com";

            if (string.IsNullOrEmpty(responderUserId))
            {
                return Unauthorized(new { message = "Responder user not found" });
            }

            var response = await _ticketService.AddTicketResponseAsync(dto, responderUserId, responderName, responderEmail);
            return CreatedAtAction(nameof(GetTicket), new { id = response.TicketId }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding response to ticket: {Id}", id);
            return StatusCode(500, new { message = "Error adding response" });
        }
    }

    #endregion

    #region Assignment Management

    /// <summary>
    /// Assign ticket to user
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignTicket(Guid id, [FromBody] string assignedToUserId)
    {
        try
        {
            await _ticketService.AssignTicketAsync(id, assignedToUserId);
            return Ok(new { message = "Ticket assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning ticket: {Id}", id);
            return StatusCode(500, new { message = "Error assigning ticket" });
        }
    }

    #endregion

    #region Reporting (UC-13.10: Generate Support Report)

    /// <summary>
    /// Generate support report (UC-13.10)
    /// Admin/Super Admin only
    /// </summary>
    [HttpPost("report")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(SupportTicketReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupportTicketReportDto>> GenerateReport(
        [FromBody] SupportReportRequestDto request)
    {
        try
        {
            var report = await _ticketService.GenerateReportAsync(request.StartDate, request.EndDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating support report");
            return StatusCode(500, new { message = "Error generating report" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get my tickets count
    /// Available to all authenticated users
    /// </summary>
    [HttpGet("my-tickets/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetMyTicketsCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var count = await _ticketService.GetMyTicketsCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my tickets count");
            return StatusCode(500, new { message = "Error retrieving count" });
        }
    }

    /// <summary>
    /// Get all tickets count
    /// Admin/Super Admin only
    /// </summary>
    [HttpGet("all-tickets/count")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetAllTicketsCount()
    {
        try
        {
            var count = await _ticketService.GetAllTicketsCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tickets count");
            return StatusCode(500, new { message = "Error retrieving count" });
        }
    }

    /// <summary>
    /// Get unsolved tickets count
    /// Admin/Super Admin only
    /// </summary>
    [HttpGet("unsolved/count")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnsolvedTicketsCount()
    {
        try
        {
            var count = await _ticketService.GetUnsolvedTicketsCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unsolved tickets count");
            return StatusCode(500, new { message = "Error retrieving count" });
        }
    }

    #endregion

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    #endregion
}

/// <summary>
/// Support Report Request DTO
/// </summary>
public class SupportReportRequestDto
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
