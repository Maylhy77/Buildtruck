namespace BuildTruckStatsService.Stats.Application.Internal.CommandServices;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.Commands;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;
using BuildTruckStatsService.Stats.Domain.Repositories;
using BuildTruckStatsService.Stats.Domain.Services;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckShared.Domain.Repositories;

public class StatsCommandService : IStatsCommandService
{
    private readonly IStatsRepository _statsRepository;
    private readonly IStatsHistoryRepository _historyRepository;
    private readonly IProjectContextService _projectContextService;
    private readonly IUserContextService _userContextService;
    private readonly IPersonnelContextService _personnelContextService;
    private readonly IMaterialContextService _materialContextService;
    private readonly IIncidentContextService _incidentContextService;
    private readonly IMachineryContextService _machineryContextService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatsCommandService> _logger;

    public StatsCommandService(
        IStatsRepository statsRepository,
        IStatsHistoryRepository historyRepository,
        IProjectContextService projectContextService,
        IUserContextService userContextService,
        IPersonnelContextService personnelContextService,
        IMaterialContextService materialContextService,
        IIncidentContextService incidentContextService,
        IMachineryContextService machineryContextService,
        IUnitOfWork unitOfWork,
        ILogger<StatsCommandService> logger)
    {
        _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _projectContextService = projectContextService ?? throw new ArgumentNullException(nameof(projectContextService));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _personnelContextService = personnelContextService ?? throw new ArgumentNullException(nameof(personnelContextService));
        _materialContextService = materialContextService ?? throw new ArgumentNullException(nameof(materialContextService));
        _incidentContextService = incidentContextService ?? throw new ArgumentNullException(nameof(incidentContextService));
        _machineryContextService = machineryContextService ?? throw new ArgumentNullException(nameof(machineryContextService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManagerStats> Handle(CalculateManagerStatsCommand command)
    {
        try
        {
            _logger.LogInformation("Calculating stats for manager {ManagerId} for period {Period}",
                command.ManagerId, command.Period);

            if (!command.IsValid())
            {
                var errors = string.Join(", ", command.GetValidationErrors());
                throw new ArgumentException($"Invalid command: {errors}");
            }

            if (!await _userContextService.IsValidManagerAsync(command.ManagerId))
                throw new ArgumentException($"Manager {command.ManagerId} not found or invalid");

            if (!command.ForceRecalculation)
            {
                var existing = await _statsRepository.FindByManagerIdAndPeriodAsync(command.ManagerId, command.Period);
                if (existing != null && existing.IsRecent())
                {
                    _logger.LogDebug("Returning existing recent stats for manager {ManagerId}", command.ManagerId);
                    return existing;
                }
            }

            var projectIds = await _projectContextService.GetManagerProjectIdsAsync(command.ManagerId);
            if (!projectIds.Any())
            {
                _logger.LogWarning("No projects found for manager {ManagerId}", command.ManagerId);
                return CreateEmptyStats(command.ManagerId, command.Period);
            }

            var projectMetrics = await _projectContextService.GetProjectMetricsAsync(command.ManagerId, command.Period);
            var personnelMetrics = await _personnelContextService.GetPersonnelMetricsAsync(projectIds, command.Period);
            var incidentMetrics = await _incidentContextService.GetIncidentMetricsAsync(projectIds, command.Period);
            var materialMetrics = await _materialContextService.GetMaterialMetricsAsync(projectIds, command.Period);
            var machineryMetrics = await _machineryContextService.GetMachineryMetricsAsync(projectIds, command.Period);

            var stats = new ManagerStats(
                command.ManagerId,
                command.Period,
                projectMetrics,
                personnelMetrics,
                incidentMetrics,
                materialMetrics,
                machineryMetrics);

            var existing2 = await _statsRepository.FindByManagerIdAndPeriodAsync(command.ManagerId, command.Period);
            if (existing2 != null)
            {
                existing2.RecalculateMetrics(projectMetrics, personnelMetrics, incidentMetrics, materialMetrics, machineryMetrics);
                _statsRepository.Update(existing2);
                stats = existing2;
            }
            else
            {
                await _statsRepository.AddAsync(stats);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully calculated stats for manager {ManagerId} with score {Score}",
                command.ManagerId, stats.OverallPerformanceScore);

            if (command.SaveHistory)
                await Handle(SaveStatsHistoryCommand.Automatic(stats.Id, command.HistoryNotes));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating stats for manager {ManagerId}", command.ManagerId);
            throw;
        }
    }

    public async Task<StatsHistory> Handle(SaveStatsHistoryCommand command)
    {
        try
        {
            _logger.LogInformation("Saving stats history for ManagerStats {ManagerStatsId}", command.ManagerStatsId);

            if (!command.IsValid())
            {
                var errors = string.Join(", ", command.GetValidationErrors());
                throw new ArgumentException($"Invalid command: {errors}");
            }

            var managerStats = await _statsRepository.FindByIdAsync(command.ManagerStatsId);
            if (managerStats == null)
                throw new ArgumentException($"ManagerStats {command.ManagerStatsId} not found");

            if (!command.OverwriteExisting)
            {
                var existingSnapshot = await _historyRepository.FindExistingSnapshotAsync(
                    managerStats.ManagerId,
                    managerStats.Period.PeriodType,
                    DateTime.UtcNow.AddHours(-5));

                if (existingSnapshot != null)
                {
                    _logger.LogDebug("Snapshot already exists for manager {ManagerId}", managerStats.ManagerId);
                    return existingSnapshot;
                }
            }

            var history = new StatsHistory(managerStats, command.Notes ?? string.Empty, command.IsManualSnapshot);
            await _historyRepository.AddAsync(history);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully saved stats history {HistoryId} for manager {ManagerId}",
                history.Id, managerStats.ManagerId);

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving stats history for ManagerStats {ManagerStatsId}", command.ManagerStatsId);
            throw;
        }
    }

    public async Task<ManagerStats> RecalculateManagerStats(int managerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var period = startDate.HasValue && endDate.HasValue
            ? StatsPeriod.Custom(startDate.Value, endDate.Value)
            : StatsPeriod.CurrentMonth();

        var command = CalculateManagerStatsCommand.WithForceRecalculation(managerId, period, true);
        return await Handle(command);
    }

    public async Task<ManagerStats> UpdateManagerStats(int managerStatsId)
    {
        var existing = await _statsRepository.FindByIdAsync(managerStatsId);
        if (existing == null)
            throw new ArgumentException($"ManagerStats {managerStatsId} not found");

        var command = CalculateManagerStatsCommand.WithForceRecalculation(existing.ManagerId, existing.Period);
        return await Handle(command);
    }

    public async Task<StatsHistory> CreateManualSnapshot(int managerStatsId, string notes)
    {
        var command = SaveStatsHistoryCommand.Manual(managerStatsId, notes);
        return await Handle(command);
    }

    public async Task<int> DeleteOutdatedStats(int daysOld = 90)
    {
        _logger.LogInformation("Deleting stats older than {DaysOld} days", daysOld);
        var deleted = await _statsRepository.DeleteOldStatsAsync(daysOld);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Deleted {Count} outdated stats", deleted);
        return deleted;
    }

    public async Task<int> ArchiveOldHistory(int daysOld = 365)
    {
        _logger.LogInformation("Archiving history older than {DaysOld} days", daysOld);
        var archived = await _historyRepository.ArchiveOldSnapshotsAsync(daysOld);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Archived {Count} old history snapshots", archived);
        return archived;
    }

    public async Task<IEnumerable<ManagerStats>> BulkCalculateStats(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null)
    {
        var results = new List<ManagerStats>();
        var period = startDate.HasValue && endDate.HasValue
            ? StatsPeriod.Custom(startDate.Value, endDate.Value)
            : StatsPeriod.CurrentMonth();

        foreach (var managerId in managerIds)
        {
            try
            {
                var command = new CalculateManagerStatsCommand(managerId, period, false, true);
                results.Add(await Handle(command));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating stats for manager {ManagerId} in bulk operation", managerId);
            }
        }

        return results;
    }

    public Task ScheduleAutomaticCalculation(int managerId, string cronExpression)
    {
        _logger.LogInformation("Scheduling automatic calculation for manager {ManagerId} with expression {Cron}",
            managerId, cronExpression);
        throw new NotImplementedException("Automatic scheduling not implemented yet");
    }

    private static ManagerStats CreateEmptyStats(int managerId, StatsPeriod period)
    {
        return new ManagerStats(
            managerId,
            period,
            ProjectMetrics.FromCounts(0, 0, 0),
            PersonnelMetrics.FromCounts(0, 0),
            IncidentMetrics.FromCounts(0, 0, 0),
            MaterialMetrics.FromCounts(0, 0, 0, 0m),
            MachineryMetrics.FromCounts(0, 0, 0));
    }
}
