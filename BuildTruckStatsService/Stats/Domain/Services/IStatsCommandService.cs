namespace BuildTruckStatsService.Stats.Domain.Services;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Domain.Model.Commands;

public interface IStatsCommandService
{
    Task<ManagerStats> Handle(CalculateManagerStatsCommand command);
    Task<StatsHistory> Handle(SaveStatsHistoryCommand command);
    Task<ManagerStats> RecalculateManagerStats(int managerId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ManagerStats> UpdateManagerStats(int managerStatsId);
    Task<StatsHistory> CreateManualSnapshot(int managerStatsId, string notes);
    Task<int> DeleteOutdatedStats(int daysOld = 90);
    Task<int> ArchiveOldHistory(int daysOld = 365);
    Task<IEnumerable<ManagerStats>> BulkCalculateStats(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null);
    Task ScheduleAutomaticCalculation(int managerId, string cronExpression);
}
