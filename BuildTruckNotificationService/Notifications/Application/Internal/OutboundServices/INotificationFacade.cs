using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;

public interface INotificationFacade
{
    Task<int> CreateNotificationAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, string? actionText = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null);

    Task<int> CreateBulkNotificationAsync(List<int> userIds, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, int? relatedProjectId = null);

    Task<List<Notification>> GetUserNotificationsAsync(int userId, int page = 1, int size = 20,
        bool? isRead = null, NotificationContext? context = null, int? relatedProjectId = null);

    Task<Dictionary<string, object>> GetNotificationSummaryAsync(int userId);
    Task<Notification?> GetNotificationByIdAsync(int notificationId, int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(List<int> notificationIds, int userId);
    Task CleanOldNotificationsAsync(int userId, int daysOld = 30);
    Task<List<NotificationPreference>> GetUserPreferencesAsync(int userId);
    Task UpdatePreferenceAsync(int userId, NotificationContext context, bool inAppEnabled, bool emailEnabled, NotificationPriority minimumPriority);
    Task CreateDefaultPreferencesAsync(int userId);
}
