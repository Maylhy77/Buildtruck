namespace BuildTruckStatsService.Stats.Application.Internal.QueryServices;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.Queries;
using BuildTruckStatsService.Stats.Domain.Repositories;
using BuildTruckStatsService.Stats.Domain.Services;
using BuildTruckStatsService.Stats.Application.ACL.Services;

public class StatsQueryService : IStatsQueryService
{
    private readonly IStatsRepository _statsRepository;
    private readonly IStatsHistoryRepository _historyRepository;
    private readonly IUserContextService _userContextService;
    private readonly IPersonnelContextService _personnelContextService;
    private readonly ILogger<StatsQueryService> _logger;

    public StatsQueryService(
        IStatsRepository statsRepository,
        IStatsHistoryRepository historyRepository,
        IUserContextService userContextService,
        IPersonnelContextService personnelContextService,
        ILogger<StatsQueryService> logger)
    {
        _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _personnelContextService = personnelContextService ?? throw new ArgumentNullException(nameof(personnelContextService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManagerStats?> Handle(GetManagerStatsQuery query)
    {
        try
        {
            _logger.LogDebug("Getting stats for manager {ManagerId}", query.ManagerId);

            if (!query.IsValid())
            {
                var errors = string.Join(", ", query.GetValidationErrors());
                throw new ArgumentException($"Invalid query: {errors}");
            }

            if (!await _userContextService.IsValidManagerAsync(query.ManagerId))
            {
                _logger.LogWarning("Manager {ManagerId} not found or invalid", query.ManagerId);
                return null;
            }

            ManagerStats? stats;
            if (query.Period != null)
                stats = await _statsRepository.FindByManagerIdAndPeriodAsync(query.ManagerId, query.Period);
            else
                stats = await _statsRepository.FindMostRecentByManagerIdAsync(query.ManagerId);

            if (stats == null)
            {
                _logger.LogDebug("No stats found for manager {ManagerId}", query.ManagerId);
                return null;
            }

            _logger.LogDebug("Found stats for manager {ManagerId} with score {Score}",
                query.ManagerId, stats.OverallPerformanceScore);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for manager {ManagerId}", query.ManagerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> Handle(GetStatsHistoryQuery query)
    {
        try
        {
            _logger.LogDebug("Getting stats history for manager {ManagerId}", query.ManagerId);

            if (!query.IsValid())
            {
                var errors = string.Join(", ", query.GetValidationErrors());
                throw new ArgumentException($"Invalid query: {errors}");
            }

            IEnumerable<StatsHistory> history;

            if (query.StartDate.HasValue && query.EndDate.HasValue)
                history = await _historyRepository.FindByManagerIdAndDateRangeAsync(query.ManagerId, query.StartDate.Value, query.EndDate.Value);
            else if (!string.IsNullOrEmpty(query.PeriodType))
                history = await _historyRepository.FindByManagerIdAndPeriodTypeAsync(query.ManagerId, query.PeriodType);
            else if (query.Limit.HasValue)
                history = await _historyRepository.FindRecentByManagerIdAsync(query.ManagerId, query.Limit.Value);
            else
                history = await _historyRepository.FindByManagerIdAsync(query.ManagerId);

            if (!query.IncludeManualSnapshots)
                history = history.Where(h => !h.IsManualSnapshot);

            history = query.OrderByNewest
                ? history.OrderByDescending(h => h.SnapshotDate)
                : history.OrderBy(h => h.SnapshotDate);

            if (query.Limit.HasValue && !query.StartDate.HasValue && string.IsNullOrEmpty(query.PeriodType))
                history = history.Take(query.Limit.Value);

            var result = history.ToList();
            _logger.LogDebug("Found {Count} history entries for manager {ManagerId}", result.Count, query.ManagerId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats history for manager {ManagerId}", query.ManagerId);
            throw;
        }
    }

    public async Task<ManagerStats?> GetCurrentStats(int managerId)
    {
        var query = GetManagerStatsQuery.ForCurrentMonth(managerId);
        return await Handle(query);
    }

    public async Task<StatsComparison?> GetStatsComparison(int managerId, DateTime p1Start, DateTime p1End, DateTime p2Start, DateTime p2End)
    {
        try
        {
            var history1 = await _historyRepository.FindByManagerIdAndDateRangeAsync(managerId, p1Start, p1End);
            var history2 = await _historyRepository.FindByManagerIdAndDateRangeAsync(managerId, p2Start, p2End);

            var snapshot1 = history1.OrderByDescending(h => h.SnapshotDate).FirstOrDefault();
            var snapshot2 = history2.OrderByDescending(h => h.SnapshotDate).FirstOrDefault();

            if (snapshot1 == null || snapshot2 == null)
            {
                _logger.LogWarning("Cannot compare: missing snapshots for manager {ManagerId}", managerId);
                return null;
            }

            return snapshot1.CompareTo(snapshot2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats comparison for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrends(int managerId, int months = 12)
    {
        try
        {
            return await _historyRepository.GetPerformanceTrendAsync(managerId, months);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trends for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrends(int managerId, int months = 12)
    {
        try
        {
            return await _historyRepository.GetSafetyTrendAsync(managerId, months);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting safety trends for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> GetTopPerformers(int count = 10, string? periodType = null)
    {
        try
        {
            var allStats = await _statsRepository.ListAsync();
            if (!string.IsNullOrEmpty(periodType))
                allStats = allStats.Where(s => s.Period.PeriodType == periodType);

            return allStats
                .Where(s => s.IsCurrentPeriod)
                .OrderByDescending(s => s.OverallPerformanceScore)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performers");
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> GetManagersWithCriticalAlerts()
    {
        try
        {
            return await _statsRepository.FindWithCriticalAlertsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting managers with critical alerts");
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetSystemSummary()
    {
        try
        {
            var systemSummary = await _statsRepository.GetSystemWideSummaryAsync();
            var repositoryStats = await _historyRepository.GetRepositoryStatsAsync();

            return new Dictionary<string, object>(systemSummary)
            {
                ["HistoryStats"] = repositoryStats,
                ["GeneratedAt"] = DateTime.UtcNow.AddHours(-5),
                ["TotalManagers"] = systemSummary.GetValueOrDefault("TotalManagers", 0),
                ["AveragePerformance"] = systemSummary.GetValueOrDefault("AveragePerformance", 0m),
                ["ManagersWithAlerts"] = systemSummary.GetValueOrDefault("ManagersWithAlerts", 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system summary");
            throw;
        }
    }

    public async Task<IEnumerable<(int ManagerId, decimal Score, int Rank)>> GetManagerRankings()
    {
        try
        {
            var currentStats = await _statsRepository.ListAsync();
            var recentStats = currentStats
                .Where(s => s.IsCurrentPeriod)
                .OrderByDescending(s => s.OverallPerformanceScore)
                .ToList();

            return recentStats
                .Select((stats, index) => (stats.ManagerId, stats.OverallPerformanceScore, index + 1))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting manager rankings");
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> SearchStats(string? grade = null, decimal? minScore = null, decimal? maxScore = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var stats = await _statsRepository.ListAsync();

            if (!string.IsNullOrEmpty(grade))
                stats = stats.Where(s => s.PerformanceGrade == grade);
            if (minScore.HasValue)
                stats = stats.Where(s => s.OverallPerformanceScore >= minScore.Value);
            if (maxScore.HasValue)
                stats = stats.Where(s => s.OverallPerformanceScore <= maxScore.Value);
            if (fromDate.HasValue)
                stats = stats.Where(s => s.CalculatedAt >= fromDate.Value);
            if (toDate.HasValue)
                stats = stats.Where(s => s.CalculatedAt <= toDate.Value);

            return stats.OrderByDescending(s => s.CalculatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stats");
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetManagerDashboard(int managerId)
    {
        try
        {
            var currentStats = await GetCurrentStats(managerId);
            var recentHistory = await _historyRepository.FindRecentByManagerIdAsync(managerId, 5);
            var performanceTrend = await GetPerformanceTrends(managerId, 6);
            var safetyTrend = await GetSafetyTrends(managerId, 6);

            return new Dictionary<string, object>
            {
                ["ManagerId"] = managerId,
                ["CurrentStats"] = currentStats!,
                ["RecentHistory"] = recentHistory.ToList(),
                ["PerformanceTrend"] = performanceTrend.ToList(),
                ["SafetyTrend"] = safetyTrend.ToList(),
                ["LastUpdated"] = currentStats?.CalculatedAt ?? DateTime.MinValue,
                ["HasCriticalAlerts"] = currentStats?.HasCriticalAlerts() ?? false,
                ["OverallStatus"] = currentStats?.GetOverallStatus() ?? "Sin datos"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<Dictionary<int, List<string>>> GetManagersAlerts(IEnumerable<int> managerIds)
    {
        try
        {
            var alerts = new Dictionary<int, List<string>>();
            foreach (var managerId in managerIds)
            {
                var stats = await GetCurrentStats(managerId);
                alerts[managerId] = stats?.Alerts ?? new List<string>();
            }
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting managers alerts");
            throw;
        }
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ExportStatsData(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var exportData = new List<Dictionary<string, object>>();
            foreach (var managerId in managerIds)
            {
                IEnumerable<StatsHistory> history;
                if (startDate.HasValue && endDate.HasValue)
                    history = await _historyRepository.FindByManagerIdAndDateRangeAsync(managerId, startDate.Value, endDate.Value);
                else
                    history = await _historyRepository.FindRecentByManagerIdAsync(managerId, 12);

                foreach (var snapshot in history)
                {
                    var data = snapshot.GetSummaryData();
                    data["ManagerId"] = managerId;
                    exportData.Add(data);
                }
            }
            return exportData.OrderBy(d => d["SnapshotDate"]).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting stats data");
            throw;
        }
    }
}
