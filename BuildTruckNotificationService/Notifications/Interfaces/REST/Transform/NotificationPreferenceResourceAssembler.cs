using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Commands;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Transform;

public static class NotificationPreferenceResourceAssembler
{
    public static NotificationPreferenceResource ToResourceFromEntity(NotificationPreference preference) =>
        new(
            preference.Id,
            preference.UserId,
            preference.Context.Value,
            preference.InAppEnabled,
            preference.EmailEnabled,
            preference.MinimumPriority.Value,
            preference.CreatedDate?.DateTime ?? DateTime.MinValue,
            preference.UpdatedDate?.DateTime
        );

    public static IEnumerable<NotificationPreferenceResource> ToResourceFromEntity(
        IEnumerable<NotificationPreference> preferences) => preferences.Select(ToResourceFromEntity);

    public static UpdatePreferenceCommand ToCommandFromResource(int userId, UpdatePreferenceResource resource) =>
        new(
            userId,
            NotificationContext.FromString(resource.Context),
            resource.InAppEnabled,
            resource.EmailEnabled,
            NotificationPriority.FromString(resource.MinimumPriority)
        );
}
