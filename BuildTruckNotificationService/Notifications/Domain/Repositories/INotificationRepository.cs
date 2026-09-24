using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IEnumerable<Notification>> FindByUserIdWithFiltersAsync(int userId, int page, int size,
        bool? isRead, NotificationContext? context, NotificationPriority? minimumPriority, int? relatedProjectId);
    Task<int> CountUnreadByUserIdAsync(int userId);
    Task<Dictionary<string, object>> GetSummaryByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> FindByProjectIdAsync(int projectId);
    Task BulkMarkAsReadAsync(List<int> notificationIds, int userId);
    Task DeleteOldNotificationsAsync(int userId, int daysOld);
    Task<IEnumerable<Notification>> FindByTypeAndContextAsync(NotificationType type, NotificationContext context, DateTime since);
}
