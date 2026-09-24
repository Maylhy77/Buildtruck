using BuildTruckConfigurationService.Configurations.Application.ACL.Services;

namespace BuildTruckConfigurationService.Configurations.Infrastructure.ACL;

/// <summary>
/// Integration seam for the Notifications bounded context.
/// The monolith does not currently expose an endpoint for creating system notifications.
/// </summary>
public class NotificationContextService : INotificationContextService
{
    public Task NotifyConfigurationCreatedAsync(int userId) => Task.CompletedTask;
}
