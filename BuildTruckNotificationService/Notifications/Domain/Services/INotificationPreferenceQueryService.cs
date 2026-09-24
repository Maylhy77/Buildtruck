using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;

namespace BuildTruckNotificationService.Notifications.Domain.Services;

public interface INotificationPreferenceQueryService
{
    Task<IEnumerable<NotificationPreference>> Handle(GetUserPreferencesQuery query);
}
