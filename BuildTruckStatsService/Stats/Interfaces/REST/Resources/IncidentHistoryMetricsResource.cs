namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record IncidentHistoryMetricsResource(
    int TotalIncidents,
    int CriticalIncidents,
    decimal SafetyScore
);
