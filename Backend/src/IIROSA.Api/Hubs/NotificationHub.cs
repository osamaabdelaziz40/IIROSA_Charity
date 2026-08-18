using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IIROSA.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// Anonymous access allowed for broadcasting notifications to all clients
/// </summary>
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client {ConnectionId} connected to NotificationHub. UserId: {UserId}",
            connectionId, userId ?? "Anonymous");

        // If authenticated, add to user-specific group for targeted notifications
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
            _logger.LogInformation("Added connection {ConnectionId} to group User_{{UserId}}", connectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (exception != null)
        {
            _logger.LogError(exception, "Client {ConnectionId} disconnected from NotificationHub with error",
                connectionId);
        }
        else
        {
            _logger.LogInformation("Client {ConnectionId} disconnected from NotificationHub. UserId: {UserId}",
                connectionId, userId ?? "Anonymous");
        }

        // Remove from user-specific group if authenticated
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, $"User_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific notification group
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Leave a specific notification group
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }
}
