namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record ProjectHistoryMetricsResource(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects,
    decimal ProjectCompletionRate
);
