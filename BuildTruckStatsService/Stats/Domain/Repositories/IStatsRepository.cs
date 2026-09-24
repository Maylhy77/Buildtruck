namespace BuildTruckStatsService.Stats.Domain.Repositories;

using BuildTruckShared.Domain.Repositories;
using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IStatsRepository : IBaseRepository<ManagerStats>
{
    Task<ManagerStats?> FindByManagerIdAsync(int managerId);
    Task<ManagerStats?> FindByManagerIdAndPeriodAsync(int managerId, StatsPeriod period);
    Task<ManagerStats?> FindMostRecentByManagerIdAsync(int managerId);
    Task<IEnumerable<ManagerStats>> FindOutdatedStatsAsync(int hoursOld = 24);
    Task<IEnumerable<ManagerStats>> FindByPerformanceGradeAsync(string grade);
    Task<IEnumerable<ManagerStats>> FindWithCriticalAlertsAsync();
    Task<IEnumerable<ManagerStats>> FindByPerformanceRangeAsync(decimal minScore, decimal maxScore);
    Task<IEnumerable<ManagerStats>> FindByCalculationDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ExistsForManagerAndPeriodAsync(int managerId, StatsPeriod period);
    Task<int> DeleteOldStatsAsync(int daysOld = 90);
    Task<Dictionary<string, object>> GetSystemWideSummaryAsync();
    Task UpdateStatsAsync(ManagerStats stats);
}
