namespace BuildTruckStatsService.Stats.Application.ACL.Services;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IIncidentContextService
{
    Task<IncidentMetrics> GetIncidentMetricsAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetIncidentsBySeverityAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetIncidentsByTypeAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetIncidentsByStatusAsync(List<int> projectIds, StatsPeriod period);
    Task<int> GetTotalIncidentsCountAsync(List<int> projectIds, StatsPeriod period);
    Task<int> GetCriticalIncidentsCountAsync(List<int> projectIds, StatsPeriod period);
    Task<int> GetOpenIncidentsCountAsync(List<int> projectIds);
    Task<int> GetResolvedIncidentsCountAsync(List<int> projectIds, StatsPeriod period);
    Task<decimal> GetAverageResolutionTimeAsync(List<int> projectIds, StatsPeriod period);
}
