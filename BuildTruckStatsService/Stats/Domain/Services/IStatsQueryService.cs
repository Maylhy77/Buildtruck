namespace BuildTruckStatsService.Stats.Domain.Services;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.Queries;

public interface IStatsQueryService
{
    Task<ManagerStats?> Handle(GetManagerStatsQuery query);
    Task<IEnumerable<StatsHistory>> Handle(GetStatsHistoryQuery query);
    Task<ManagerStats?> GetCurrentStats(int managerId);
    Task<StatsComparison?> GetStatsComparison(int managerId, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);
    Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrends(int managerId, int months = 12);
    Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrends(int managerId, int months = 12);
    Task<IEnumerable<ManagerStats>> GetTopPerformers(int count = 10, string? periodType = null);
    Task<IEnumerable<ManagerStats>> GetManagersWithCriticalAlerts();
    Task<Dictionary<string, object>> GetSystemSummary();
    Task<IEnumerable<(int ManagerId, decimal Score, int Rank)>> GetManagerRankings();
    Task<IEnumerable<ManagerStats>> SearchStats(string? grade = null, decimal? minScore = null, decimal? maxScore = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<string, object>> GetManagerDashboard(int managerId);
    Task<Dictionary<int, List<string>>> GetManagersAlerts(IEnumerable<int> managerIds);
    Task<IEnumerable<Dictionary<string, object>>> ExportStatsData(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null);
}
