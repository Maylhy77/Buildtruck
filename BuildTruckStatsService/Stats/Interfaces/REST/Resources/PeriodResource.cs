namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record PeriodResource(
    DateTime StartDate,
    DateTime EndDate,
    string PeriodType,
    string DisplayName,
    int TotalDays,
    bool IsCurrentPeriod
);
