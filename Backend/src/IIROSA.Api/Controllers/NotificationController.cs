using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using IIROSA.Api.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Anonymous notification controller for sending real-time notifications via SignalR
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class NotificationController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Send notification to all connected users
    /// </summary>
    /// <param name="request">Notification details</param>
    /// <returns>Success response</returns>
    [HttpPost("send-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendToAll([FromBody] BroadcastNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { success = false, message = "Message is required" });
        }

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            id = Guid.NewGuid(),
            type = request.Type ?? "info",
            title = request.Title ?? "Notification",
            message = request.Message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Broadcast notification sent to all users: {Message}", request.Message);

        return Ok(new
        {
            success = true,
            message = "Notification sent to all users",
            data = new
            {
                type = request.Type,
                title = request.Title,
                message = request.Message,
                timestamp = DateTime.UtcNow
            }
        });
    }

    /// <summary>
    /// Send notification to a specific user by their ID
    /// </summary>
    /// <param name="userId">The user ID to send notification to</param>
    /// <param name="request">Notification details</param>
    /// <returns>Success response</returns>
    [HttpPost("send-user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendToUser(string userId, [FromBody] UserNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { success = false, message = "User ID is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { success = false, message = "Message is required" });
        }

        // Send to the user's specific group
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
        {
            id = Guid.NewGuid(),
            type = request.Type ?? "info",
            title = request.Title ?? "Notification",
            message = request.Message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Notification sent to user {UserId}: {Message}", userId, request.Message);

        return Ok(new
        {
            success = true,
            message = $"Notification sent to user {userId}",
            data = new
            {
                userId = userId,
                type = request.Type,
                title = request.Title,
                message = request.Message,
                timestamp = DateTime.UtcNow
            }
        });
    }
}

/// <summary>
/// Request model for broadcast notifications
/// </summary>
public class BroadcastNotificationRequest
{
    /// <summary>
    /// Notification type: success, error, warning, info
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Notification title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Notification message content
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request model for user-specific notifications
/// </summary>
public class UserNotificationRequest
{
    /// <summary>
    /// Notification type: success, error, warning, info
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Notification title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Notification message content
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
