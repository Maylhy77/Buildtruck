namespace BuildTruckStatsService.Stats.Domain.Model.Queries;

public record GetStatsHistoryQuery(
    int ManagerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? PeriodType = null,
    int? Limit = null,
    bool OrderByNewest = true,
    bool IncludeManualSnapshots = true)
{
    public bool IsValid()
    {
        return ManagerId > 0 &&
               (StartDate == null || EndDate == null || StartDate <= EndDate) &&
               (Limit == null || Limit > 0);
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (ManagerId <= 0) errors.Add("ManagerId must be greater than 0");
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate) errors.Add("StartDate cannot be after EndDate");
        if (Limit.HasValue && Limit <= 0) errors.Add("Limit must be greater than 0");
        return errors;
    }

    public static GetStatsHistoryQuery Recent(int managerId, int days = 30)
    {
        var endDate = DateTime.UtcNow.AddHours(-5);
        var startDate = endDate.AddDays(-days);
        return new GetStatsHistoryQuery(managerId, startDate, endDate, null, null, true, true);
    }

    public static GetStatsHistoryQuery ForMonth(int managerId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new GetStatsHistoryQuery(managerId, startDate, endDate, "CURRENT_MONTH", null, true, true);
    }

    public static GetStatsHistoryQuery ForYear(int managerId, int year)
    {
        return new GetStatsHistoryQuery(managerId, new DateTime(year, 1, 1), new DateTime(year, 12, 31),
            "CURRENT_YEAR", null, true, true);
    }

    public static GetStatsHistoryQuery ForComparison(int managerId, int snapshotCount = 5)
    {
        return new GetStatsHistoryQuery(managerId, null, null, null, snapshotCount, true, false);
    }

    public static GetStatsHistoryQuery ForPeriodType(int managerId, string periodType, int limit = 12)
    {
        return new GetStatsHistoryQuery(managerId, null, null, periodType, limit, true, true);
    }

    public static GetStatsHistoryQuery ForTrends(int managerId, int quarters = 4)
    {
        return new GetStatsHistoryQuery(managerId, null, null, "CURRENT_QUARTER", quarters, true, false);
    }
}
