using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Commands;

public record CreateNotificationCommand(
    int UserId,
    NotificationType Type,
    NotificationContext Context,
    NotificationPriority Priority,
    string Title,
    string Message,
    UserRole TargetRole,
    NotificationScope Scope,
    int? RelatedProjectId = null,
    int? RelatedEntityId = null,
    string? RelatedEntityType = null,
    string? ActionUrl = null,
    string? ActionText = null,
    string? MetadataJson = null
);
