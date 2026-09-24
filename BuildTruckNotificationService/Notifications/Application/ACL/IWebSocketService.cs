using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;

namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IWebSocketService
{
    Task SendToUserAsync(int userId, Notification notification);
    Task SendToGroupAsync(string groupName, Notification notification);
    Task SendUnreadCountUpdateAsync(int userId, int unreadCount);
    Task SendNotificationReadAsync(int userId, int notificationId);
}
