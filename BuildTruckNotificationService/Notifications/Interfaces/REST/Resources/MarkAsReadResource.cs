namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

public record MarkAsReadResource(int? NotificationId, List<int>? NotificationIds)
{
    public bool IsSingle => NotificationId.HasValue && (NotificationIds == null || !NotificationIds.Any());
    public bool IsBulk => NotificationIds != null && NotificationIds.Any();
    public bool IsValid => IsSingle || IsBulk;
}
