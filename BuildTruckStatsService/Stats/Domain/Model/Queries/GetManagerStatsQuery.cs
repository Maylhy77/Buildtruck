namespace BuildTruckStatsService.Stats.Domain.Model.Queries;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public record GetManagerStatsQuery(
    int ManagerId,
    StatsPeriod? Period = null,
    bool IncludeAlerts = true,
    bool IncludeRecommendations = true)
{
    public bool IsValid() => ManagerId > 0;

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (ManagerId <= 0) errors.Add("ManagerId must be greater than 0");
        return errors;
    }

    public static GetManagerStatsQuery ForCurrentMonth(int managerId)
    {
        return new GetManagerStatsQuery(managerId, StatsPeriod.CurrentMonth(), true, true);
    }

    public static GetManagerStatsQuery ForPeriod(int managerId, StatsPeriod period)
    {
        return new GetManagerStatsQuery(managerId, period, true, true);
    }

    public static GetManagerStatsQuery ForDateRange(int managerId, DateTime startDate, DateTime endDate)
    {
        return new GetManagerStatsQuery(managerId, StatsPeriod.Custom(startDate, endDate), true, true);
    }

    public static GetManagerStatsQuery Minimal(int managerId, StatsPeriod? period = null)
    {
        return new GetManagerStatsQuery(managerId, period ?? StatsPeriod.CurrentMonth(), false, false);
    }
}
