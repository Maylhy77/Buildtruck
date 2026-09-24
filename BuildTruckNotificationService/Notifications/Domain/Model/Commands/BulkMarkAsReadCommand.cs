namespace BuildTruckNotificationService.Notifications.Domain.Model.Commands;

public record BulkMarkAsReadCommand(List<int> NotificationIds, int UserId);
