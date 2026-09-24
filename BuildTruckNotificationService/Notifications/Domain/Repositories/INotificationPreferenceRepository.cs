using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Domain.Repositories;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference>
{
    Task<IEnumerable<NotificationPreference>> FindByUserIdAsync(int userId);
    Task<NotificationPreference?> FindByUserIdAndContextAsync(int userId, NotificationContext context);
    Task<bool> ExistsByUserIdAndContextAsync(int userId, NotificationContext context);
}
