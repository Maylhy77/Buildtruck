namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

/// <summary>
/// Value Object representing a time period for statistics calculation
/// </summary>
public class StatsPeriod
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string PeriodType { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    // Constructor for EF Core
    protected StatsPeriod() { }

    public StatsPeriod(DateTime startDate, DateTime endDate, string periodType = "CUSTOM")
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date");

        StartDate = startDate;
        EndDate = endDate;
        PeriodType = periodType;
        DisplayName = GenerateDisplayName();
    }

    public static StatsPeriod CurrentMonth()
    {
        var now = DateTime.Now;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new StatsPeriod(startDate, endDate, "CURRENT_MONTH");
    }

    public static StatsPeriod CurrentQuarter()
    {
        var now = DateTime.Now;
        var quarter = (now.Month - 1) / 3 + 1;
        var startDate = new DateTime(now.Year, (quarter - 1) * 3 + 1, 1);
        var endDate = startDate.AddMonths(3).AddDays(-1);
        return new StatsPeriod(startDate, endDate, "CURRENT_QUARTER");
    }

    public static StatsPeriod CurrentYear()
    {
        var now = DateTime.Now;
        var startDate = new DateTime(now.Year, 1, 1);
        var endDate = new DateTime(now.Year, 12, 31);
        return new StatsPeriod(startDate, endDate, "CURRENT_YEAR");
    }

    public static StatsPeriod LastNDays(int days)
    {
        var endDate = DateTime.Now.Date;
        var startDate = endDate.AddDays(-days + 1);
        return new StatsPeriod(startDate, endDate, $"LAST_{days}_DAYS");
    }

    public static StatsPeriod Custom(DateTime startDate, DateTime endDate)
    {
        return new StatsPeriod(startDate, endDate, "CUSTOM");
    }

    public int GetTotalDays() => (EndDate - StartDate).Days + 1;

    public bool Contains(DateTime date) => date.Date >= StartDate.Date && date.Date <= EndDate.Date;

    public bool IsCurrentPeriod()
    {
        var today = DateTime.Now.Date;
        return Contains(today);
    }

    private string GenerateDisplayName()
    {
        return PeriodType switch
        {
            "CURRENT_MONTH" => $"{StartDate:MMMM yyyy}",
            "CURRENT_QUARTER" => $"Q{(StartDate.Month - 1) / 3 + 1} {StartDate.Year}",
            "CURRENT_YEAR" => $"{StartDate.Year}",
            "CUSTOM" => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}",
            _ when PeriodType.StartsWith("LAST_") && PeriodType.EndsWith("_DAYS") =>
                $"Últimos {GetTotalDays()} días",
            _ => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}"
        };
    }

    public override string ToString() => DisplayName;

    public override bool Equals(object? obj)
    {
        if (obj is not StatsPeriod other) return false;
        return StartDate == other.StartDate && EndDate == other.EndDate;
    }

    public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);
}
