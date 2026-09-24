namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IProjectContextService
{
    Task<IEnumerable<int>> GetAllActiveProjectsAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<string> GetProjectNameAsync(int projectId);
    Task<int> GetProjectManagerIdAsync(int projectId);
    Task<IEnumerable<int>> GetProjectMemberIdsAsync(int projectId);
}
