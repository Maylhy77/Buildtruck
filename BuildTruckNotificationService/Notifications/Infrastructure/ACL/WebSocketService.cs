using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Interfaces.WebSocket;
using Microsoft.AspNetCore.SignalR;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

public class WebSocketService : IWebSocketService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public WebSocketService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(int userId, Notification notification)
    {
        var data = new
        {
            id = notification.Id,
            title = notification.Content.Title,
            message = notification.Content.Message,
            type = notification.Type.Value,
            context = notification.Context.Value,
            priority = notification.Priority.Value,
            actionUrl = notification.Content.ActionUrl,
            actionText = notification.Content.ActionText,
            iconClass = notification.Content.IconClass,
            createdAt = notification.CreatedDate,
            isRead = notification.IsRead
        };

        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NewNotification", data);
    }

    public async Task SendToGroupAsync(string groupName, Notification notification)
    {
        var data = new
        {
            id = notification.Id,
            title = notification.Content.Title,
            message = notification.Content.Message,
            type = notification.Type.Value,
            context = notification.Context.Value,
            priority = notification.Priority.Value,
            actionUrl = notification.Content.ActionUrl,
            createdAt = notification.CreatedDate
        };

        await _hubContext.Clients.Group(groupName).SendAsync("NewNotification", data);
    }

    public async Task SendUnreadCountUpdateAsync(int userId, int unreadCount)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdate", new { unreadCount });
    }

    public async Task SendNotificationReadAsync(int userId, int notificationId)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationRead", new { notificationId });
    }
}
