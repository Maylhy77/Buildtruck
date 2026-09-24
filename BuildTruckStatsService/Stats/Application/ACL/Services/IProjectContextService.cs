namespace BuildTruckStatsService.Stats.Application.ACL.Services;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IProjectContextService
{
    Task<ProjectMetrics> GetProjectMetricsAsync(int managerId, StatsPeriod period);
    Task<Dictionary<string, int>> GetProjectsByStatusAsync(int managerId, StatsPeriod period);
    Task<bool> ManagerHasProjectsAsync(int managerId);
    Task<int> GetActiveProjectsCountAsync(int managerId);
    Task<int> GetCompletedProjectsCountAsync(int managerId, StatsPeriod period);
    Task<int> GetOverdueProjectsCountAsync(int managerId);
    Task<List<int>> GetManagerProjectIdsAsync(int managerId);
}
