using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Queries;

public record GetNotificationsByUserQuery(
    int UserId,
    int Page = 1,
    int Size = 20,
    bool? IsRead = null,
    NotificationContext? Context = null,
    NotificationPriority? MinimumPriority = null,
    int? RelatedProjectId = null
);
