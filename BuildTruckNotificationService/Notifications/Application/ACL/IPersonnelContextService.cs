namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IPersonnelContextService
{
    Task<decimal> GetAttendanceRateAsync(int projectId);
    Task<string?> GetPersonnelNameAsync(int personnelId);
}
