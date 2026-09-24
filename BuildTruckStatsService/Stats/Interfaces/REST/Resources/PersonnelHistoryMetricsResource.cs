namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record PersonnelHistoryMetricsResource(
    int TotalPersonnel,
    int ActivePersonnel,
    decimal PersonnelActiveRate,
    decimal PersonnelEfficiencyScore
);
