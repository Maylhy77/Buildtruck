using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Commands;

public record UpdatePreferenceCommand(
    int UserId,
    NotificationContext Context,
    bool InAppEnabled,
    bool EmailEnabled,
    NotificationPriority MinimumPriority
);
