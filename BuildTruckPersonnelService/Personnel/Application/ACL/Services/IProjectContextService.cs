namespace BuildTruckPersonnelService.Personnel.Application.ACL.Services;

public interface IProjectContextService
{
    Task<bool> ProjectExistsAsync(int projectId);
    Task<string?> GetProjectNameAsync(int projectId);
    Task<bool> UserCanAccessProjectAsync(int userId, int projectId);
}
