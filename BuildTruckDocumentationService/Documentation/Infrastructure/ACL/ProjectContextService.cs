using BuildTruckDocumentationService.Documentation.Application.ACL.Services;
using BuildTruckDocumentationService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckDocumentationService.Documentation.Infrastructure.ACL;

/// <summary>
/// ACL Adapter to communicate with Project context
/// </summary>
public class ProjectContextService : IProjectContextService
{
    private readonly IProjectFacade _projectFacade;

    public ProjectContextService(IProjectFacade projectFacade)
    {
        _projectFacade = projectFacade;
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        try
        {
            return await _projectFacade.ExistsByIdAsync(projectId);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string?> GetProjectNameAsync(int projectId)
    {
        try
        {
            var project = await _projectFacade.GetProjectByIdAsync(projectId);
            return project?.Name;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> UserCanAccessProjectAsync(int userId, int projectId)
    {
        try
        {
            return await _projectFacade.UserHasAccessToProjectAsync(userId, projectId);
        }
        catch (Exception)
        {
            return false;
        }
    }
}