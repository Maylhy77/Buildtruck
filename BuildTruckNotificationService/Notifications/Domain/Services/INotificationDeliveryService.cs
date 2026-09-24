using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Services;

public interface INotificationDeliveryService
{
    Task DeliverAsync(Notification notification, NotificationChannel channel);
    Task RetryFailedDeliveriesAsync();
    Task<bool> CanDeliverAsync(Notification notification, NotificationChannel channel);
}
