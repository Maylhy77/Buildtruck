using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Commands;

public record DeliverNotificationCommand(int NotificationId, NotificationChannel Channel);
