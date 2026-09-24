namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record MachineryHistoryMetricsResource(
    int TotalMachinery,
    int ActiveMachinery,
    decimal MachineryAvailabilityRate,
    decimal MachineryEfficiencyScore
);
