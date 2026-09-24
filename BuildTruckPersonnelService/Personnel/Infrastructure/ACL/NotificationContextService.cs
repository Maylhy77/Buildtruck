using BuildTruckPersonnelService.Personnel.Application.ACL.Services;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.ACL;

/// <summary>
/// Stub implementation — the Notifications bounded context lives in the monolith.
/// No HTTP endpoint for incoming notifications exists yet.
/// </summary>
public class NotificationContextService : INotificationContextService
{
    public Task NotifyPersonnelAddedAsync(int projectId, string personnelName, int personnelId) => Task.CompletedTask;
    public Task NotifyPersonnelUpdatedAsync(int projectId, string personnelName) => Task.CompletedTask;
    public Task NotifyPersonnelRemovedAsync(int projectId, string personnelName) => Task.CompletedTask;
}
