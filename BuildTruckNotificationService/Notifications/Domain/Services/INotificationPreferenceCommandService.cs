using BuildTruckNotificationService.Notifications.Domain.Model.Commands;

namespace BuildTruckNotificationService.Notifications.Domain.Services;

public interface INotificationPreferenceCommandService
{
    Task Handle(UpdatePreferenceCommand command);
    Task CreateDefaultPreferencesAsync(int userId);
    Task ResetToDefaultsAsync(int userId);
}
