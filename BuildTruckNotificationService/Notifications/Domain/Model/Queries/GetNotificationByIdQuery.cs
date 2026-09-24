namespace BuildTruckNotificationService.Notifications.Domain.Model.Queries;

public record GetNotificationByIdQuery(int NotificationId, int UserId);
