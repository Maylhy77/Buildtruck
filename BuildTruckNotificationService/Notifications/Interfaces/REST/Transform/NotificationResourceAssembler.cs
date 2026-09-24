using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Transform;

public static class NotificationResourceAssembler
{
    public static NotificationResource ToResourceFromEntity(Notification notification) =>
        new(
            notification.Id,
            notification.UserId,
            notification.Type.Value,
            notification.Context.Value,
            notification.Priority.Value,
            notification.Content.Title,
            notification.Content.Message,
            notification.Content.ActionUrl,
            notification.Content.ActionText,
            notification.Content.IconClass,
            notification.Status.Value,
            notification.Scope.Value,
            notification.TargetRole.Value,
            notification.RelatedProjectId,
            notification.RelatedEntityId,
            notification.RelatedEntityType,
            notification.IsRead,
            notification.ReadAt,
            notification.CreatedDate?.DateTime ?? DateTime.MinValue,
            notification.UpdatedDate?.DateTime
        );

    public static IEnumerable<NotificationResource> ToResourceFromEntity(IEnumerable<Notification> notifications) =>
        notifications.Select(ToResourceFromEntity);

    public static NotificationSummaryResource ToSummaryResource(Dictionary<string, object> summary) =>
        new(
            (int)summary["unreadCount"],
            ((Dictionary<string, object>)summary["byContext"]).ToDictionary(k => k.Key, k => (int)k.Value),
            (DateTime)summary["lastUpdated"]
        );
}
