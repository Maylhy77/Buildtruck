using BuildTruckShared.Domain.Repositories;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Application.Internal.CommandServices;
using BuildTruckStatsService.Stats.Application.Internal.QueryServices;
using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.Commands;
using BuildTruckStatsService.Stats.Domain.Model.Queries;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;
using BuildTruckStatsService.Stats.Domain.Repositories;
using BuildTruckStatsService.Stats.Interfaces.REST.Transform;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildTruckBackend.Tests;

public class StatsTests
{
    [Fact]
    public void MachineryMetrics_CalculatesRatesAndMaintenanceAlerts()
    {
        var metrics = new MachineryMetrics(
            totalMachinery: 10,
            activeMachinery: 4,
            inMaintenanceMachinery: 4,
            inactiveMachinery: 2,
            machineryByStatus: new Dictionary<string, int> { ["active"] = 4, ["maintenance"] = 4, ["inactive"] = 2 },
            machineryByType: new Dictionary<string, int> { ["Excavadora"] = 6, ["Mixer"] = 4 },
            machineryByProject: new Dictionary<string, int> { ["Proyecto Norte"] = 7, ["Proyecto Sur"] = 3 },
            overallAvailabilityRate: 72m,
            averageMaintenanceTimeHours: 96m);

        Assert.Equal(40m, metrics.GetActiveRate());
        Assert.Equal(40m, metrics.GetMaintenanceRate());
        Assert.Equal("Bajo", metrics.GetAvailabilityStatus());
        Assert.True(metrics.NeedsMaintenance());
        Assert.Contains("Alto porcentaje de maquinaria en mantenimiento", metrics.GetMaintenanceAlerts());
        Assert.Equal("Proyecto Norte", metrics.GetProjectWithMostMachinery());
    }

    [Fact]
    public void ManagerStats_GeneratesScoreBreakdownAndCriticalAlerts()
    {
        var stats = CreateManagerStats(
            managerId: 33,
            machineryMetrics: MachineryMetrics.FromCounts(8, 3, 4),
            incidentMetrics: IncidentMetrics.FromCounts(4, 2, 2),
            materialMetrics: MaterialMetrics.FromCounts(10, 6, 2, 1200m));

        var breakdown = stats.GetScoreBreakdown();

        Assert.True(stats.HasCriticalAlerts());
        Assert.Contains(stats.Alerts, alert => alert.Contains("mantenimiento"));
        Assert.Equal(stats.OverallPerformanceScore, breakdown["General"]);
        Assert.True(breakdown.ContainsKey("Maquinaria"));
    }

    [Fact]
    public void StatsResourceAssembler_MapsManagerStatsIncludingMachineryMetrics()
    {
        var stats = CreateManagerStats(managerId: 12, machineryMetrics: MachineryMetrics.FromCounts(6, 5, 1));

        var resource = StatsResourceAssembler.ToResourceFromEntity(stats);

        Assert.Equal(12, resource.ManagerId);
        Assert.Equal(6, resource.MachineryMetrics.TotalMachinery);
        Assert.Equal(5, resource.MachineryMetrics.ActiveMachinery);
        Assert.Equal(stats.OverallPerformanceScore, resource.OverallPerformanceScore);
        Assert.Equal(stats.GetOverallStatus(), resource.OverallStatus);
        Assert.True(resource.ScoreBreakdown.ContainsKey("Maquinaria"));
    }

    [Fact]
    public async Task StatsCommandService_CalculatesStatsFromContextsAndPersists()
    {
        var statsRepository = new FakeStatsRepository();
        var historyRepository = new FakeStatsHistoryRepository();
        var projectContext = new FakeProjectContextService(projectIds: [101, 102]);
        var personnelContext = new FakePersonnelContextService();
        var incidentContext = new FakeIncidentContextService();
        var materialContext = new FakeMaterialContextService();
        var machineryContext = new FakeMachineryContextService();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateCommandService(
            statsRepository,
            historyRepository,
            projectContext,
            new FakeUserContextService(validManagers: [44]),
            personnelContext,
            materialContext,
            incidentContext,
            machineryContext,
            unitOfWork);

        var stats = await service.Handle(new CalculateManagerStatsCommand(
            ManagerId: 44,
            Period: StatsPeriod.CurrentMonth(),
            ForceRecalculation: false,
            SaveHistory: true,
            HistoryNotes: "Sprint review snapshot"));

        Assert.Equal(44, stats.ManagerId);
        Assert.Equal(1, statsRepository.AddCalls);
        Assert.Equal(1, historyRepository.AddCalls);
        Assert.Equal(2, unitOfWork.CompleteCalls);
        Assert.Equal([101, 102], machineryContext.LastProjectIds);
        Assert.Equal(5, stats.MachineryMetrics.TotalMachinery);
        Assert.True(stats.OverallPerformanceScore > 0);
    }

    [Fact]
    public async Task StatsCommandService_ReturnsRecentExistingStatsWithoutRecalculatingContexts()
    {
        var period = StatsPeriod.CurrentMonth();
        var existingStats = CreateManagerStats(managerId: 55, period: period);
        var statsRepository = new FakeStatsRepository();
        await statsRepository.AddAsync(existingStats);
        var projectContext = new FakeProjectContextService(projectIds: [9]);
        var machineryContext = new FakeMachineryContextService();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateCommandService(
            statsRepository,
            new FakeStatsHistoryRepository(),
            projectContext,
            new FakeUserContextService(validManagers: [55]),
            new FakePersonnelContextService(),
            new FakeMaterialContextService(),
            new FakeIncidentContextService(),
            machineryContext,
            unitOfWork);

        var result = await service.Handle(new CalculateManagerStatsCommand(
            ManagerId: 55,
            Period: period,
            ForceRecalculation: false));

        Assert.Same(existingStats, result);
        Assert.Equal(0, projectContext.ProjectIdsCalls);
        Assert.Equal(0, machineryContext.MetricsCalls);
        Assert.Equal(0, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task StatsQueryService_ReturnsCurrentStatsForValidManager()
    {
        var period = StatsPeriod.CurrentMonth();
        var statsRepository = new FakeStatsRepository();
        var expected = CreateManagerStats(managerId: 77, period: period);
        await statsRepository.AddAsync(expected);
        var queryService = new StatsQueryService(
            statsRepository,
            new FakeStatsHistoryRepository(),
            new FakeUserContextService(validManagers: [77]),
            new FakePersonnelContextService(),
            NullLogger<StatsQueryService>.Instance);

        var result = await queryService.Handle(GetManagerStatsQuery.ForPeriod(77, period));

        Assert.Same(expected, result);
    }

    private static StatsCommandService CreateCommandService(
        FakeStatsRepository statsRepository,
        FakeStatsHistoryRepository historyRepository,
        FakeProjectContextService projectContext,
        FakeUserContextService userContext,
        FakePersonnelContextService personnelContext,
        FakeMaterialContextService materialContext,
        FakeIncidentContextService incidentContext,
        FakeMachineryContextService machineryContext,
        FakeUnitOfWork unitOfWork) =>
        new(
            statsRepository,
            historyRepository,
            projectContext,
            userContext,
            personnelContext,
            materialContext,
            incidentContext,
            machineryContext,
            unitOfWork,
            NullLogger<StatsCommandService>.Instance);

    private static ManagerStats CreateManagerStats(
        int managerId,
        StatsPeriod? period = null,
        ProjectMetrics? projectMetrics = null,
        PersonnelMetrics? personnelMetrics = null,
        IncidentMetrics? incidentMetrics = null,
        MaterialMetrics? materialMetrics = null,
        MachineryMetrics? machineryMetrics = null) =>
        new(
            managerId,
            period ?? StatsPeriod.CurrentMonth(),
            projectMetrics ?? ProjectMetrics.FromCounts(4, 2, 2),
            personnelMetrics ?? new PersonnelMetrics(12, 10, 2, averageAttendanceRate: 92m),
            incidentMetrics ?? IncidentMetrics.FromCounts(1, 0, 0),
            materialMetrics ?? MaterialMetrics.FromCounts(12, 10, 1, 2500m),
            machineryMetrics ?? MachineryMetrics.FromCounts(6, 5, 1));

    private static void SetPrivateId(object entity, int id)
    {
        var backingField = entity.GetType().GetField(
            "<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CompleteCalls { get; private set; }

        public Task CompleteAsync()
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserContextService : IUserContextService
    {
        private readonly HashSet<int> _validManagers;

        public FakeUserContextService(IEnumerable<int> validManagers)
        {
            _validManagers = validManagers.ToHashSet();
        }

        public Task<bool> IsValidManagerAsync(int userId) => Task.FromResult(_validManagers.Contains(userId));

        public Task<Dictionary<string, object>?> GetManagerInfoAsync(int managerId) =>
            Task.FromResult<Dictionary<string, object>?>(
                _validManagers.Contains(managerId)
                    ? new Dictionary<string, object> { ["id"] = managerId, ["role"] = "Manager" }
                    : null);
    }

    private sealed class FakeProjectContextService : IProjectContextService
    {
        private readonly List<int> _projectIds;
        public int ProjectIdsCalls { get; private set; }

        public FakeProjectContextService(IEnumerable<int> projectIds)
        {
            _projectIds = projectIds.ToList();
        }

        public Task<ProjectMetrics> GetProjectMetricsAsync(int managerId, StatsPeriod period) =>
            Task.FromResult(ProjectMetrics.FromCounts(4, 2, 2));

        public Task<Dictionary<string, int>> GetProjectsByStatusAsync(int managerId, StatsPeriod period) =>
            Task.FromResult(new Dictionary<string, int> { ["Activo"] = 2, ["Completado"] = 2 });

        public Task<bool> ManagerHasProjectsAsync(int managerId) => Task.FromResult(_projectIds.Count > 0);

        public Task<int> GetActiveProjectsCountAsync(int managerId) => Task.FromResult(2);

        public Task<int> GetCompletedProjectsCountAsync(int managerId, StatsPeriod period) => Task.FromResult(2);

        public Task<int> GetOverdueProjectsCountAsync(int managerId) => Task.FromResult(0);

        public Task<List<int>> GetManagerProjectIdsAsync(int managerId)
        {
            ProjectIdsCalls++;
            return Task.FromResult(_projectIds);
        }
    }

    private sealed class FakePersonnelContextService : IPersonnelContextService
    {
        public Task<PersonnelMetrics> GetPersonnelMetricsAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(new PersonnelMetrics(20, 18, 2, averageAttendanceRate: 91m));

        public Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds) =>
            Task.FromResult(new Dictionary<string, int> { ["Operario"] = 14, ["Supervisor"] = 6 });

        public Task<int> GetActivePersonnelCountAsync(List<int> projectIds) => Task.FromResult(18);

        public Task<int> GetTotalPersonnelCountAsync(List<int> projectIds) => Task.FromResult(20);

        public Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(91m);

        public Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds) => Task.FromResult(80000m);
    }

    private sealed class FakeMaterialContextService : IMaterialContextService
    {
        public Task<MaterialMetrics> GetMaterialMetricsAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(MaterialMetrics.FromCounts(30, 25, 3, 15000m, 9000m));

        public Task<Dictionary<string, int>> GetMaterialsByCategoryAsync(List<int> projectIds) =>
            Task.FromResult(new Dictionary<string, int> { ["Cemento"] = 12, ["Acero"] = 18 });

        public Task<decimal> GetTotalMaterialCostAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(15000m);

        public Task<decimal> GetTotalUsageCostAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(9000m);

        public Task<int> GetMaterialsInStockCountAsync(List<int> projectIds) => Task.FromResult(25);

        public Task<int> GetMaterialsLowStockCountAsync(List<int> projectIds) => Task.FromResult(3);

        public Task<int> GetMaterialsOutOfStockCountAsync(List<int> projectIds) => Task.FromResult(2);

        public Task<Dictionary<string, decimal>> GetCostsByCategoryAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(new Dictionary<string, decimal> { ["Cemento"] = 6000m, ["Acero"] = 9000m });

        public Task<int> GetTotalMaterialsCountAsync(List<int> projectIds) => Task.FromResult(30);
    }

    private sealed class FakeIncidentContextService : IIncidentContextService
    {
        public Task<IncidentMetrics> GetIncidentMetricsAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(IncidentMetrics.FromCounts(2, 0, 1));

        public Task<Dictionary<string, int>> GetIncidentsBySeverityAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(new Dictionary<string, int> { ["Bajo"] = 2 });

        public Task<Dictionary<string, int>> GetIncidentsByTypeAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(new Dictionary<string, int> { ["Seguridad"] = 2 });

        public Task<Dictionary<string, int>> GetIncidentsByStatusAsync(List<int> projectIds, StatsPeriod period) =>
            Task.FromResult(new Dictionary<string, int> { ["Reportado"] = 1, ["Resuelto"] = 1 });

        public Task<int> GetTotalIncidentsCountAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(2);

        public Task<int> GetCriticalIncidentsCountAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(0);

        public Task<int> GetOpenIncidentsCountAsync(List<int> projectIds) => Task.FromResult(1);

        public Task<int> GetResolvedIncidentsCountAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(1);

        public Task<decimal> GetAverageResolutionTimeAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(12m);
    }

    private sealed class FakeMachineryContextService : IMachineryContextService
    {
        public int MetricsCalls { get; private set; }
        public List<int> LastProjectIds { get; private set; } = [];

        public Task<MachineryMetrics> GetMachineryMetricsAsync(List<int> projectIds, StatsPeriod period)
        {
            MetricsCalls++;
            LastProjectIds = projectIds.ToList();
            return Task.FromResult(MachineryMetrics.FromCounts(5, 4, 1));
        }

        public Task<Dictionary<string, int>> GetMachineryByStatusAsync(List<int> projectIds) =>
            Task.FromResult(new Dictionary<string, int> { ["active"] = 4, ["maintenance"] = 1 });

        public Task<Dictionary<string, int>> GetMachineryByTypeAsync(List<int> projectIds) =>
            Task.FromResult(new Dictionary<string, int> { ["Excavadora"] = 3, ["Mixer"] = 2 });

        public Task<int> GetTotalMachineryCountAsync(List<int> projectIds) => Task.FromResult(5);

        public Task<int> GetActiveMachineryCountAsync(List<int> projectIds) => Task.FromResult(4);

        public Task<int> GetMachineryInMaintenanceCountAsync(List<int> projectIds) => Task.FromResult(1);

        public Task<int> GetInactiveMachineryCountAsync(List<int> projectIds) => Task.FromResult(0);

        public Task<decimal> GetOverallAvailabilityRateAsync(List<int> projectIds) => Task.FromResult(80m);

        public Task<decimal> GetAverageMaintenanceTimeAsync(List<int> projectIds, StatsPeriod period) => Task.FromResult(18m);

        public Task<Dictionary<string, int>> GetMachineryByProjectAsync(List<int> projectIds) =>
            Task.FromResult(new Dictionary<string, int> { ["101"] = 3, ["102"] = 2 });
    }

    private sealed class FakeStatsRepository : IStatsRepository
    {
        private int _nextId = 1;
        public List<ManagerStats> Stats { get; } = [];
        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }

        public Task AddAsync(ManagerStats entity)
        {
            if (entity.Id == 0)
                SetPrivateId(entity, _nextId++);
            Stats.Add(entity);
            AddCalls++;
            return Task.CompletedTask;
        }

        public Task<ManagerStats?> FindByIdAsync(int id) =>
            Task.FromResult(Stats.FirstOrDefault(stats => stats.Id == id));

        public Task<ManagerStats?> FindByManagerIdAsync(int managerId) =>
            Task.FromResult(Stats.FirstOrDefault(stats => stats.ManagerId == managerId));

        public Task<ManagerStats?> FindByManagerIdAndPeriodAsync(int managerId, StatsPeriod period) =>
            Task.FromResult(Stats.FirstOrDefault(stats =>
                stats.ManagerId == managerId && stats.Period.Equals(period)));

        public Task<ManagerStats?> FindMostRecentByManagerIdAsync(int managerId) =>
            Task.FromResult(Stats
                .Where(stats => stats.ManagerId == managerId)
                .OrderByDescending(stats => stats.CalculatedAt)
                .FirstOrDefault());

        public Task<IEnumerable<ManagerStats>> FindOutdatedStatsAsync(int hoursOld = 24) =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats.Where(stats => stats.GetAgeInHours() > hoursOld));

        public Task<IEnumerable<ManagerStats>> FindByPerformanceGradeAsync(string grade) =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats.Where(stats => stats.PerformanceGrade == grade));

        public Task<IEnumerable<ManagerStats>> FindWithCriticalAlertsAsync() =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats.Where(stats => stats.HasCriticalAlerts()));

        public Task<IEnumerable<ManagerStats>> FindByPerformanceRangeAsync(decimal minScore, decimal maxScore) =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats.Where(stats =>
                stats.OverallPerformanceScore >= minScore && stats.OverallPerformanceScore <= maxScore));

        public Task<IEnumerable<ManagerStats>> FindByCalculationDateRangeAsync(DateTime startDate, DateTime endDate) =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats.Where(stats =>
                stats.CalculatedAt >= startDate && stats.CalculatedAt <= endDate));

        public Task<bool> ExistsForManagerAndPeriodAsync(int managerId, StatsPeriod period) =>
            Task.FromResult(Stats.Any(stats => stats.ManagerId == managerId && stats.Period.Equals(period)));

        public Task<int> DeleteOldStatsAsync(int daysOld = 90) => Task.FromResult(0);

        public Task<Dictionary<string, object>> GetSystemWideSummaryAsync() =>
            Task.FromResult(new Dictionary<string, object>
            {
                ["TotalManagers"] = Stats.Select(stats => stats.ManagerId).Distinct().Count(),
                ["AveragePerformance"] = Stats.Any() ? Stats.Average(stats => stats.OverallPerformanceScore) : 0m,
                ["ManagersWithAlerts"] = Stats.Count(stats => stats.HasCriticalAlerts())
            });

        public Task<IEnumerable<ManagerStats>> ListAsync() =>
            Task.FromResult<IEnumerable<ManagerStats>>(Stats);

        public void Remove(ManagerStats entity) => Stats.Remove(entity);

        public void Update(ManagerStats entity)
        {
            UpdateCalls++;
        }

        public Task UpdateStatsAsync(ManagerStats stats)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStatsHistoryRepository : IStatsHistoryRepository
    {
        private int _nextId = 1;
        public List<StatsHistory> History { get; } = [];
        public int AddCalls { get; private set; }

        public Task AddAsync(StatsHistory entity)
        {
            if (entity.Id == 0)
                SetPrivateId(entity, _nextId++);
            History.Add(entity);
            AddCalls++;
            return Task.CompletedTask;
        }

        public Task<StatsHistory?> FindByIdAsync(int id) =>
            Task.FromResult(History.FirstOrDefault(history => history.Id == id));

        public Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history => history.ManagerId == managerId));

        public Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId, int skip, int take) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history => history.ManagerId == managerId).Skip(skip).Take(take));

        public Task<IEnumerable<StatsHistory>> FindByManagerIdAndDateRangeAsync(int managerId, DateTime startDate, DateTime endDate) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history =>
                history.ManagerId == managerId && history.SnapshotDate >= startDate && history.SnapshotDate <= endDate));

        public Task<IEnumerable<StatsHistory>> FindByManagerIdAndPeriodTypeAsync(int managerId, string periodType) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history =>
                history.ManagerId == managerId && history.PeriodType == periodType));

        public Task<IEnumerable<StatsHistory>> FindRecentByManagerIdAsync(int managerId, int count = 10) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History
                .Where(history => history.ManagerId == managerId)
                .OrderByDescending(history => history.SnapshotDate)
                .Take(count));

        public Task<IEnumerable<StatsHistory>> FindForComparisonAsync(int managerId, string periodType, int count = 5) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History
                .Where(history => history.ManagerId == managerId && history.PeriodType == periodType)
                .Take(count));

        public Task<IEnumerable<StatsHistory>> FindByMonthAsync(int managerId, int year, int month) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history =>
                history.ManagerId == managerId && history.SnapshotDate.Year == year && history.SnapshotDate.Month == month));

        public Task<IEnumerable<StatsHistory>> FindByYearAsync(int managerId, int year) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history =>
                history.ManagerId == managerId && history.SnapshotDate.Year == year));

        public Task<IEnumerable<StatsHistory>> FindManualSnapshotsAsync(int managerId) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history => history.ManagerId == managerId && history.IsManualSnapshot));

        public Task<IEnumerable<StatsHistory>> FindAutomaticSnapshotsAsync(int managerId) =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history => history.ManagerId == managerId && !history.IsManualSnapshot));

        public Task<bool> ExistsForManagerStatsAsync(int managerStatsId) =>
            Task.FromResult(History.Any(history => history.ManagerStatsId == managerStatsId));

        public Task<StatsHistory?> FindExistingSnapshotAsync(int managerId, string periodType, DateTime snapshotDate) =>
            Task.FromResult(History.FirstOrDefault(history =>
                history.ManagerId == managerId &&
                history.PeriodType == periodType &&
                history.SnapshotDate.Date == snapshotDate.Date));

        public Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrendAsync(int managerId, int months = 12) =>
            Task.FromResult<IEnumerable<(DateTime Date, decimal Score)>>(
                History.Where(history => history.ManagerId == managerId)
                    .Select(history => (history.SnapshotDate, history.OverallPerformanceScore)));

        public Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrendAsync(int managerId, int months = 12) =>
            Task.FromResult<IEnumerable<(DateTime Date, decimal SafetyScore)>>(
                History.Where(history => history.ManagerId == managerId)
                    .Select(history => (history.SnapshotDate, history.SafetyScore)));

        public Task<int> DeleteOldSnapshotsAsync(int daysOld = 365) => Task.FromResult(0);

        public Task<int> ArchiveOldSnapshotsAsync(int daysOld = 365) => Task.FromResult(0);

        public Task<Dictionary<string, object>> GetRepositoryStatsAsync() =>
            Task.FromResult(new Dictionary<string, object> { ["TotalSnapshots"] = History.Count });

        public Task<IEnumerable<StatsHistory>> FindSnapshotsToArchiveAsync() =>
            Task.FromResult<IEnumerable<StatsHistory>>(History.Where(history => history.ShouldBeArchived()));

        public Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int managerId, int year) =>
            Task.FromResult(new Dictionary<string, decimal>());

        public Task<Dictionary<string, decimal>> GetQuarterlySummaryAsync(int managerId, int year) =>
            Task.FromResult(new Dictionary<string, decimal>());

        public Task<IEnumerable<StatsHistory>> ListAsync() =>
            Task.FromResult<IEnumerable<StatsHistory>>(History);

        public void Remove(StatsHistory entity) => History.Remove(entity);

        public void Update(StatsHistory entity)
        {
        }
    }
}
