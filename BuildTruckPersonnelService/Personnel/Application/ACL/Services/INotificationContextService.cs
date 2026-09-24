namespace BuildTruckPersonnelService.Personnel.Application.ACL.Services;

public interface INotificationContextService
{
    Task NotifyPersonnelAddedAsync(int projectId, string personnelName, int personnelId);
    Task NotifyPersonnelUpdatedAsync(int projectId, string personnelName);
    Task NotifyPersonnelRemovedAsync(int projectId, string personnelName);
}
