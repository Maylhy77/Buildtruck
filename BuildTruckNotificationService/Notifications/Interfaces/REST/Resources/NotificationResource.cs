namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

public record NotificationResource(
    int Id,
    int UserId,
    string Type,
    string Context,
    string Priority,
    string Title,
    string Message,
    string? ActionUrl,
    string? ActionText,
    string? IconClass,
    string Status,
    string Scope,
    string TargetRole,
    int? RelatedProjectId,
    int? RelatedEntityId,
    string? RelatedEntityType,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
