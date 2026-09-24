using System;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckMaterialsService.Materials.Infrastructure.ACL
{
    /// <summary>
    /// Implementation of project context service using Projects bounded context
    /// </summary>
    public class ProjectContextService : IProjectContextService
    {
        private readonly IProjectFacade _projectFacade;

        public ProjectContextService(IProjectFacade projectFacade)
        {
            _projectFacade = projectFacade;
        }

        public async Task<bool> ValidateProjectAccessAsync(Guid projectId)
        {
            // Convert Guid to int if needed, or return false for now
            // This method should be deprecated in favor of the int version
            return false;
        }

        public async Task<string?> GetProjectNameAsync(Guid projectId)
        {
            // Convert Guid to int if needed, or return null for now
            // This method should be deprecated in favor of the int version
            return null;
        }
        
        public async Task<bool> ValidateProjectAccessAsync(int projectId)
        {
            return await _projectFacade.ExistsByIdAsync(projectId);
        }

        public async Task<string?> GetProjectNameAsync(int projectId)
        {
            var projectInfo = await _projectFacade.GetProjectByIdAsync(projectId);
            return projectInfo?.Name;
        }
    }
}
