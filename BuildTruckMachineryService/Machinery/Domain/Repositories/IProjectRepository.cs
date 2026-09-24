using BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckMachineryService.Machinery.Domain.Repositories;

public interface IProjectRepository
{
    Task<ProjectInfo?> FindByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
}