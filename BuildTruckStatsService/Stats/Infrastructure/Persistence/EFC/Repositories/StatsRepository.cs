namespace BuildTruckStatsService.Stats.Infrastructure.Persistence.EFC.Repositories;

using BuildTruckStatsService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;
using BuildTruckStatsService.Stats.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class StatsRepository : BaseRepository<ManagerStats, StatsServiceDbContext>, IStatsRepository
{
    private readonly ILogger<StatsRepository> _logger;

    public StatsRepository(StatsServiceDbContext context, ILogger<StatsRepository> logger)
        : base(context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManagerStats?> FindByManagerIdAsync(int managerId)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .FirstOrDefaultAsync(s => s.ManagerId == managerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<ManagerStats?> FindByManagerIdAndPeriodAsync(int managerId, StatsPeriod period)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.ManagerId == managerId)
                .Where(s => s.Period.PeriodType == period.PeriodType)
                .Where(s => s.Period.StartDate == period.StartDate)
                .Where(s => s.Period.EndDate == period.EndDate)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats by manager ID {ManagerId} and period", managerId);
            throw;
        }
    }

    public async Task<ManagerStats?> FindMostRecentByManagerIdAsync(int managerId)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.ManagerId == managerId)
                .OrderByDescending(s => s.CalculatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding most recent stats by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> FindOutdatedStatsAsync(int hoursOld = 24)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-5).AddHours(-hoursOld);
            return await Context.Set<ManagerStats>()
                .Where(s => s.CalculatedAt < cutoff)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding outdated stats older than {HoursOld} hours", hoursOld);
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> FindByPerformanceGradeAsync(string grade)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.PerformanceGrade == grade)
                .OrderByDescending(s => s.CalculatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats by performance grade {Grade}", grade);
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> FindWithCriticalAlertsAsync()
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.Alerts.Any(a => a.Contains("crítico") || a.Contains("critico")))
                .OrderByDescending(s => s.CalculatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats with critical alerts");
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> FindByPerformanceRangeAsync(decimal minScore, decimal maxScore)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.OverallPerformanceScore >= minScore && s.OverallPerformanceScore <= maxScore)
                .OrderByDescending(s => s.OverallPerformanceScore)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats by performance range {Min}-{Max}", minScore, maxScore);
            throw;
        }
    }

    public async Task<IEnumerable<ManagerStats>> FindByCalculationDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.CalculatedAt >= startDate && s.CalculatedAt <= endDate)
                .OrderBy(s => s.CalculatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding stats by date range {Start}-{End}", startDate, endDate);
            throw;
        }
    }

    public async Task<bool> ExistsForManagerAndPeriodAsync(int managerId, StatsPeriod period)
    {
        try
        {
            return await Context.Set<ManagerStats>()
                .Where(s => s.ManagerId == managerId)
                .Where(s => s.Period.PeriodType == period.PeriodType)
                .Where(s => s.Period.StartDate == period.StartDate)
                .Where(s => s.Period.EndDate == period.EndDate)
                .AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stats existence for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<int> DeleteOldStatsAsync(int daysOld = 90)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-5).AddDays(-daysOld);
            var old = await Context.Set<ManagerStats>().Where(s => s.CalculatedAt < cutoff).ToListAsync();
            if (old.Any())
            {
                Context.Set<ManagerStats>().RemoveRange(old);
                await Context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old stats older than {DaysOld} days", old.Count, daysOld);
            }
            return old.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old stats older than {DaysOld} days", daysOld);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetSystemWideSummaryAsync()
    {
        try
        {
            var allStats = await Context.Set<ManagerStats>().Where(s => s.IsCurrentPeriod).ToListAsync();

            return new Dictionary<string, object>
            {
                ["TotalManagers"] = allStats.Select(s => s.ManagerId).Distinct().Count(),
                ["TotalStats"] = allStats.Count,
                ["AveragePerformance"] = allStats.Any() ? Math.Round(allStats.Average(s => s.OverallPerformanceScore), 2) : 0m,
                ["ManagersWithAlerts"] = allStats.Count(s => s.Alerts.Any()),
                ["TopPerformanceGrade"] = allStats.Any()
                    ? allStats.GroupBy(s => s.PerformanceGrade).OrderByDescending(g => g.Count()).First().Key
                    : "N/A",
                ["LastCalculated"] = allStats.Any() ? allStats.Max(s => s.CalculatedAt) : DateTime.MinValue,
                ["PerformanceDistribution"] = allStats.GroupBy(s => s.PerformanceGrade).ToDictionary(g => g.Key, g => g.Count())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system-wide summary");
            throw;
        }
    }

    public async Task UpdateStatsAsync(ManagerStats stats)
    {
        try
        {
            Context.Set<ManagerStats>().Update(stats);
            await Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stats for manager {ManagerId}", stats.ManagerId);
            throw;
        }
    }
}
