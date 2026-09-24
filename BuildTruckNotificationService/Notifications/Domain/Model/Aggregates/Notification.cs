using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;

public partial class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationContext Context { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public NotificationContent Content { get; private set; }
    public NotificationStatus Status { get; private set; }
    public NotificationScope Scope { get; private set; }
    public UserRole TargetRole { get; private set; }
    public int? RelatedProjectId { get; private set; }
    public int? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public Notification()
    {
        Type = NotificationType.SystemNotification;
        Context = NotificationContext.System;
        Priority = NotificationPriority.Normal;
        Content = new NotificationContent("System Notification", "Default message");
        Status = NotificationStatus.Unread;
        Scope = NotificationScope.User;
        TargetRole = UserRole.Manager;
        IsRead = false;
    }

    public Notification(int userId, NotificationType type, NotificationContext context,
        NotificationPriority priority, NotificationContent content, UserRole targetRole,
        NotificationScope scope, int? relatedProjectId = null, int? relatedEntityId = null,
        string? relatedEntityType = null, string? metadataJson = null)
    {
        UserId = userId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        TargetRole = targetRole ?? throw new ArgumentNullException(nameof(targetRole));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Status = NotificationStatus.Unread;
        RelatedProjectId = relatedProjectId;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        MetadataJson = metadataJson;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            Status = NotificationStatus.Read;
        }
    }

    public void Archive() => Status = NotificationStatus.Archived;

    public bool IsHighPriority() => Priority.Level >= NotificationPriority.High.Level;
    public bool IsCritical() => Priority.Level >= NotificationPriority.Critical.Level;
    public bool IsOlderThan(TimeSpan timespan) => CreatedDate?.DateTime < DateTime.UtcNow.Subtract(timespan);
    public bool BelongsToProject(int projectId) => RelatedProjectId == projectId;
    public bool ShouldSendEmailImmediate() => IsCritical() || Priority.RequiresEmailByDefault;
    public bool CanBeDeleted() => IsRead && IsOlderThan(TimeSpan.FromDays(30));

    public Dictionary<string, object>? GetMetadata()
    {
        if (string.IsNullOrEmpty(MetadataJson)) return null;
        try { return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson); }
        catch { return null; }
    }

    public void SetMetadata(Dictionary<string, object>? metadata)
    {
        MetadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null;
    }
}
