using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Domain.Repositories;

public interface INotificationDeliveryRepository : IBaseRepository<NotificationDelivery>
{
    Task<IEnumerable<NotificationDelivery>> FindRetryableDeliveriesAsync();
    Task<IEnumerable<NotificationDelivery>> FindByNotificationIdAsync(int notificationId);
    Task<NotificationDelivery?> FindByNotificationIdAndChannelAsync(int notificationId, NotificationChannel channel);
    Task<Dictionary<string, int>> GetDeliveryStatsAsync();
}
