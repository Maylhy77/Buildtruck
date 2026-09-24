using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface ISharedEmailService
{
    Task SendNotificationEmailAsync(string email, string fullName, NotificationType type,
        string title, string message, string? actionUrl = null);
    Task SendDigestEmailAsync(string email, string fullName, List<Notification> notifications, DateTime date);
    Task SendCriticalNotificationEmailAsync(string email, string fullName, string title,
        string message, string projectName, string? actionUrl = null);
}
