namespace BuildTruckStatsService.Stats.Domain.Repositories;

using BuildTruckShared.Domain.Repositories;
using BuildTruckStatsService.Stats.Domain.Model.Aggregates;

public interface IStatsHistoryRepository : IBaseRepository<StatsHistory>
{
    Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId);
    Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId, int skip, int take);
    Task<IEnumerable<StatsHistory>> FindByManagerIdAndDateRangeAsync(int managerId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<StatsHistory>> FindByManagerIdAndPeriodTypeAsync(int managerId, string periodType);
    Task<IEnumerable<StatsHistory>> FindRecentByManagerIdAsync(int managerId, int count = 10);
    Task<IEnumerable<StatsHistory>> FindForComparisonAsync(int managerId, string periodType, int count = 5);
    Task<IEnumerable<StatsHistory>> FindByMonthAsync(int managerId, int year, int month);
    Task<IEnumerable<StatsHistory>> FindByYearAsync(int managerId, int year);
    Task<IEnumerable<StatsHistory>> FindManualSnapshotsAsync(int managerId);
    Task<IEnumerable<StatsHistory>> FindAutomaticSnapshotsAsync(int managerId);
    Task<bool> ExistsForManagerStatsAsync(int managerStatsId);
    Task<StatsHistory?> FindExistingSnapshotAsync(int managerId, string periodType, DateTime snapshotDate);
    Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrendAsync(int managerId, int months = 12);
    Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrendAsync(int managerId, int months = 12);
    Task<int> DeleteOldSnapshotsAsync(int daysOld = 365);
    Task<int> ArchiveOldSnapshotsAsync(int daysOld = 365);
    Task<Dictionary<string, object>> GetRepositoryStatsAsync();
    Task<IEnumerable<StatsHistory>> FindSnapshotsToArchiveAsync();
    Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int managerId, int year);
    Task<Dictionary<string, decimal>> GetQuarterlySummaryAsync(int managerId, int year);
}
