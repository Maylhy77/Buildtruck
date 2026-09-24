using BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckMachineryService.Machinery.Application.ACL.Services;

public interface IProjectContextService
{
    Task<ProjectInfo?> GetProjectByIdAsync(int projectId);
    Task<bool> ExistsAsync(int projectId);
}