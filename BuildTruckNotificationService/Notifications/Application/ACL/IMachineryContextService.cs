namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IMachineryContextService
{
    Task<int> GetActiveMachineryCountAsync(int projectId);
    Task<string?> GetMachineryNameAsync(int machineryId);
}
