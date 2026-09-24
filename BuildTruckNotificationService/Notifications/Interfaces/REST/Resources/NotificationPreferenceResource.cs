namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

public record NotificationPreferenceResource(
    int Id,
    int UserId,
    string Context,
    bool InAppEnabled,
    bool EmailEnabled,
    string MinimumPriority,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
