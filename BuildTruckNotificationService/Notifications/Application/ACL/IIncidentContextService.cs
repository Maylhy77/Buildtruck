namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IIncidentContextService
{
    Task<int> GetOpenIncidentsCountAsync(int projectId);
    Task<string?> GetIncidentTitleAsync(int incidentId);
}
