using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;

namespace BuildTruckNotificationService.Notifications.Application.Internal.QueryServices;

public class NotificationPreferenceQueryService : INotificationPreferenceQueryService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public NotificationPreferenceQueryService(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    public async Task<IEnumerable<NotificationPreference>> Handle(GetUserPreferencesQuery query)
    {
        return await _preferenceRepository.FindByUserIdAsync(query.UserId);
    }
}
