using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Services.Interfaces;

namespace IIROSA.Api.Controllers;

/// <summary>
/// User Impersonation API Controller
/// Implements use cases UC-19.1 through UC-19.8
/// Only accessible by Super Admin and Admin roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImpersonationController : ControllerBase
{
    private readonly IImpersonationService _impersonationService;
    private readonly ILogger<ImpersonationController> _logger;

    public ImpersonationController(
        IImpersonationService impersonationService,
        ILogger<ImpersonationController> logger)
    {
        _impersonationService = impersonationService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : Guid.Empty;

    /// <summary>
    /// Start impersonation session (UC-19.1, UC-19.5)
    /// </summary>
    [HttpPost("start")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<StartImpersonationResponse>> StartImpersonation([FromBody] StartImpersonationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TargetUsername))
            {
                return BadRequest(new { message = "Target username is required." });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _impersonationService.StartImpersonationAsync(
                CurrentUserId,
                request.TargetUsername,
                ipAddress,
                userAgent,
                HttpContext.RequestAborted);

            _logger.LogInformation(
                "Impersonation started: {ImpersonatorId} -> {TargetUser}",
                CurrentUserId,
                request.TargetUsername);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting impersonation");
            return StatusCode(500, new { message = "An error occurred while starting impersonation" });
        }
    }

    /// <summary>
    /// End impersonation session and return to original account (UC-19.2)
    /// </summary>
    [HttpPost("end")]
    public async Task<ActionResult<EndImpersonationResponse>> EndImpersonation([FromBody] EndImpersonationRequest? request = null)
    {
        try
        {
            // Get session ID from either request body or claims
            var sessionId = request?.SessionId ?? Guid.Empty;
            if (sessionId == Guid.Empty)
            {
                var sessionClaim = User.FindFirstValue("ImpersonationSessionId");
                if (Guid.TryParse(sessionClaim, out var sid))
                {
                    sessionId = sid;
                }
            }

            if (sessionId == Guid.Empty)
            {
                return BadRequest(new { message = "No active impersonation session found." });
            }

            var result = await _impersonationService.EndImpersonationAsync(sessionId, HttpContext.RequestAborted);

            _logger.LogInformation(
                "Impersonation ended: SessionId={SessionId}, Duration={Duration}s",
                sessionId,
                result.SessionDurationSeconds);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending impersonation");
            return StatusCode(500, new { message = "An error occurred while ending impersonation" });
        }
    }

    /// <summary>
    /// Get all active impersonation sessions (UC-19.3)
    /// Super Admin only
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<List<ActiveImpersonationSessionDto>>> GetActiveSessions()
    {
        try
        {
            var sessions = await _impersonationService.GetActiveSessionsAsync(HttpContext.RequestAborted);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500, new { message = "An error occurred while retrieving active sessions" });
        }
    }

    /// <summary>
    /// Terminate an active session (admin override) (UC-19.4)
    /// Super Admin only
    /// </summary>
    [HttpPost("terminate/{sessionId}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult> TerminateSession(Guid sessionId, [FromBody] TerminateSessionRequest? request = null)
    {
        try
        {
            await _impersonationService.TerminateSessionAsync(
                sessionId,
                CurrentUserId,
                request?.Reason ?? "Admin Override",
                HttpContext.RequestAborted);

            _logger.LogWarning(
                "Impersonation session {SessionId} terminated by {TerminatedBy}",
                sessionId,
                CurrentUserId);

            return Ok(new { message = "Session terminated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred while terminating session" });
        }
    }

    /// <summary>
    /// Search for users to impersonate (UC-19.5)
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserSearchResult>>> SearchUsers([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return BadRequest(new { message = "Search term must be at least 3 characters." });
            }

            var results = await _impersonationService.SearchUsersAsync(term, CurrentUserId, HttpContext.RequestAborted);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return StatusCode(500, new { message = "An error occurred while searching users" });
        }
    }

    /// <summary>
    /// Get impersonation history (UC-19.6)
    /// Super Admin only
    /// </summary>
    [HttpPost("history")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<List<ImpersonationSessionHistoryDto>>> GetHistory([FromBody] ImpersonationHistoryFilter? filter = null)
    {
        try
        {
            var history = await _impersonationService.GetHistoryAsync(filter ?? new(), HttpContext.RequestAborted);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impersonation history");
            return StatusCode(500, new { message = "An error occurred while retrieving history" });
        }
    }

    /// <summary>
    /// Get current impersonation status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ImpersonationStatusDto>> GetStatus()
    {
        try
        {
            var status = await _impersonationService.GetStatusAsync(CurrentUserId, HttpContext.RequestAborted);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impersonation status");
            return StatusCode(500, new { message = "An error occurred while retrieving status" });
        }
    }

    /// <summary>
    /// Get impersonation settings (UC-19.8)
    /// Super Admin only
    /// </summary>
    [HttpGet("settings")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<ImpersonationSettingsDto>> GetSettings()
    {
        try
        {
            var settings = await _impersonationService.GetSettingsAsync(HttpContext.RequestAborted);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impersonation settings");
            return StatusCode(500, new { message = "An error occurred while retrieving settings" });
        }
    }

    /// <summary>
    /// Update impersonation settings (UC-19.8)
    /// Super Admin only
    /// </summary>
    [HttpPut("settings")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult> UpdateSettings([FromBody] ImpersonationSettingsDto settings)
    {
        try
        {
            await _impersonationService.UpdateSettingsAsync(settings, HttpContext.RequestAborted);
            _logger.LogInformation("Impersonation settings updated by {UserId}", CurrentUserId);
            return Ok(new { message = "Settings updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating impersonation settings");
            return StatusCode(500, new { message = "An error occurred while updating settings" });
        }
    }

    /// <summary>
    /// Increment action counter (UC-19.7)
    /// Internal endpoint called during impersonation
    /// </summary>
    [HttpPost("increment-actions")]
    [Authorize]
    public async Task<ActionResult> IncrementActions([FromBody] IncrementActionsDto request)
    {
        try
        {
            await _impersonationService.IncrementActionCountAsync(request.SessionId, HttpContext.RequestAborted);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing action count");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Validate if current user can impersonate target user
    /// </summary>
    [HttpGet("validate/{targetUserId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> ValidateImpersonation(Guid targetUserId)
    {
        try
        {
            var (canImpersonate, reason) = await _impersonationService.ValidateImpersonationAsync(
                CurrentUserId,
                targetUserId,
                HttpContext.RequestAborted);

            return Ok(new { canImpersonate, reason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating impersonation");
            return StatusCode(500, new { message = "An error occurred while validating impersonation" });
        }
    }
}

// Request DTOs
public class EndImpersonationRequest
{
    public Guid SessionId { get; set; }
}

public class IncrementActionsDto
{
    public Guid SessionId { get; set; }
}
