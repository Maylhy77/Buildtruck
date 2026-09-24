namespace BuildTruckStatsService.Stats.Application.ACL.Services;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IPersonnelContextService
{
    Task<PersonnelMetrics> GetPersonnelMetricsAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds);
    Task<int> GetActivePersonnelCountAsync(List<int> projectIds);
    Task<int> GetTotalPersonnelCountAsync(List<int> projectIds);
    Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds, StatsPeriod period);
    Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds);
}
