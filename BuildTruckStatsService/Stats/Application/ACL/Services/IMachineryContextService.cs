namespace BuildTruckStatsService.Stats.Application.ACL.Services;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IMachineryContextService
{
    Task<MachineryMetrics> GetMachineryMetricsAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetMachineryByStatusAsync(List<int> projectIds);
    Task<Dictionary<string, int>> GetMachineryByTypeAsync(List<int> projectIds);
    Task<int> GetTotalMachineryCountAsync(List<int> projectIds);
    Task<int> GetActiveMachineryCountAsync(List<int> projectIds);
    Task<int> GetMachineryInMaintenanceCountAsync(List<int> projectIds);
    Task<int> GetInactiveMachineryCountAsync(List<int> projectIds);
    Task<decimal> GetOverallAvailabilityRateAsync(List<int> projectIds);
    Task<decimal> GetAverageMaintenanceTimeAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetMachineryByProjectAsync(List<int> projectIds);
}
