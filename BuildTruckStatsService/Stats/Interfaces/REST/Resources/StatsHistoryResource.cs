namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record StatsHistoryResource(
    int Id,
    int ManagerId,
    int ManagerStatsId,
    PeriodResource Period,
    DateTime SnapshotDate,
    string PeriodType,
    decimal OverallPerformanceScore,
    string PerformanceGrade,
    ProjectHistoryMetricsResource ProjectMetrics,
    PersonnelHistoryMetricsResource PersonnelMetrics,
    IncidentHistoryMetricsResource IncidentMetrics,
    MaterialHistoryMetricsResource MaterialMetrics,
    MachineryHistoryMetricsResource MachineryMetrics,
    string DataSource,
    string Notes,
    bool IsManualSnapshot,
    string PeriodDescription,
    Dictionary<string, object> SummaryData,
    string SnapshotSummary,
    bool IsFromToday,
    bool IsFromThisWeek,
    int AgeInDays,
    string FormattedSnapshotDate,
    string RelativeSnapshotTime,
    bool ShouldBeArchived
);
