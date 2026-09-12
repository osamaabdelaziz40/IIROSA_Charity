using IIROSA.Application.DTOs.Notifications;
using IIROSA.Application.Interfaces;
using IIROSA.Api.Hubs;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Notification API Controller (UC-NTF web notifications epic).
///
/// Admin/SuperAdmin compose and push a web notification to specific users and/or
/// whole charities; every push is stored in NotificationsLog and delivered live
/// over SignalR to the resolved audience. Any authenticated user reads their own
/// notifications through <c>my</c>; edit mode offers a resend of the stored row.
/// Delivery happens here in the API layer — the service stays hub-agnostic and
/// hands back the resolved DeliveredToUserIds.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationsLogService _notificationsLogService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _notificationHubContext;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationsLogService notificationsLogService,
        UserManager<ApplicationUser> userManager,
        IHubContext<NotificationHub> notificationHubContext,
        ILogger<NotificationController> logger)
    {
        _notificationsLogService = notificationsLogService;
        _userManager = userManager;
        _notificationHubContext = notificationHubContext;
        _logger = logger;
    }

    private string? CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    // ========== Reads ==========

    /// <summary>
    /// The admin register — every pushed notification, newest first (UC-NTF list).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogPagedResult>> GetNotifications(
        [FromQuery] NotificationsLogFilterDto filter)
    {
        try
        {
            var result = await _notificationsLogService.GetFilteredAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications");
            return StatusCode(500, new { message = "An error occurred while retrieving notifications" });
        }
    }

    /// <summary>
    /// Register statistics for the band above the admin notifications grid (UC-NTF list) —
    /// same register semantics as the list read above.
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogStatisticsDto>> GetStatistics()
    {
        try
        {
            return Ok(await _notificationsLogService.GetStatisticsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving notification statistics" });
        }
    }

    /// <summary>
    /// The recipient's read (UC-NTF my notifications): every notification whose
    /// audience contains the caller directly or through their charity.
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<NotificationsLogPagedResult>> GetMyNotifications(
        [FromQuery] NotificationsLogFilterDto filter)
    {
        try
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { message = "Unable to resolve the current user" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            var result = await _notificationsLogService.GetForUserAsync(userGuid, user?.CharityId, filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving my notifications");
            return StatusCode(500, new { message = "An error occurred while retrieving my notifications" });
        }
    }

    /// <summary>
    /// Detail read for the edit screen (UC-NTF edit).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogListDto>> GetNotification(Guid id)
    {
        try
        {
            var notification = await _notificationsLogService.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification {NotificationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the notification" });
        }
    }

    // ========== Writes ==========

    /// <summary>
    /// Create and push (UC-NTF push): persist the log row, then deliver it live
    /// over SignalR to every resolved recipient.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogListDto>> CreateNotification([FromBody] CreateNotificationsLogDto model)
    {
        try
        {
            var result = await _notificationsLogService.CreateAsync(model);
            await PushAsync(result);
            return CreatedAtAction(nameof(GetNotification), new { id = result.Id }, result);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating notification");
            return StatusCode(500, new { message = "An error occurred while creating the notification" });
        }
    }

    /// <summary>
    /// Update a stored notification's content/audience (UC-NTF edit) — no push
    /// until it is explicitly resent.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogListDto>> UpdateNotification(Guid id, [FromBody] UpdateNotificationsLogDto model)
    {
        try
        {
            model.Id = id;
            var result = await _notificationsLogService.UpdateAsync(model);
            return Ok(result);
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
            _logger.LogError(ex, "Error occurred while updating notification {NotificationId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the notification" });
        }
    }

    /// <summary>
    /// Resend (UC-NTF resend): re-push the stored content to a freshly resolved
    /// audience and bump the delivery bookkeeping.
    /// </summary>
    [HttpPost("{id}/resend")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<NotificationsLogListDto>> ResendNotification(Guid id)
    {
        try
        {
            var result = await _notificationsLogService.ResendAsync(id);
            await PushAsync(result);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resending notification {NotificationId}", id);
            return StatusCode(500, new { message = "An error occurred while resending the notification" });
        }
    }

    // ========== Live delivery ==========

    /// <summary>
    /// Push one stored notification to its resolved audience. The payload shape
    /// matches the frontend SignalRService 'ReceiveNotification' contract.
    /// A hub failure must not fail the request — the row is already persisted.
    /// </summary>
    private async Task PushAsync(NotificationsLogListDto notification)
    {
        if (notification.DeliveredToUserIds == null || notification.DeliveredToUserIds.Count == 0)
        {
            _logger.LogWarning("Notification {NotificationId} resolved to an empty audience — nothing to push",
                notification.Id);
            return;
        }

        try
        {
            await _notificationHubContext.Clients
                .Users(notification.DeliveredToUserIds)
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = "info",
                    timestamp = notification.LastSentOn ?? notification.CreatedOn
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR push failed for notification {NotificationId} — the row stays stored",
                notification.Id);
        }
    }
}
