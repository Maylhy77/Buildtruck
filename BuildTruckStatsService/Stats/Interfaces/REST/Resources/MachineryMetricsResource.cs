namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record MachineryMetricsResource(
    int TotalMachinery,
    int ActiveMachinery,
    int InMaintenanceMachinery,
    int InactiveMachinery,
    Dictionary<string, int> MachineryByStatus,
    Dictionary<string, int> MachineryByType,
    Dictionary<string, int> MachineryByProject,
    decimal OverallAvailabilityRate,
    decimal AverageMaintenanceTimeHours,
    decimal ActiveRate,
    decimal MaintenanceRate,
    decimal InactiveRate,
    decimal OperationalRate,
    string AvailabilityStatus,
    bool HasHighAvailability,
    bool NeedsMaintenance,
    string MostCommonStatus,
    string MostCommonType,
    string ProjectWithMostMachinery,
    decimal EfficiencyScore,
    string MachinerySummary,
    string MaintenanceSummary,
    List<string> MaintenanceAlerts
);
