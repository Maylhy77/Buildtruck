namespace BuildTruckDocumentationService.Documentation.Application.ACL.Services;

/// <summary>
/// Anti-Corruption Layer service to communicate with Project context
/// </summary>
public interface IProjectContextService
{
    Task<bool> ProjectExistsAsync(int projectId);
    Task<string?> GetProjectNameAsync(int projectId);
    Task<bool> UserCanAccessProjectAsync(int userId, int projectId);
}