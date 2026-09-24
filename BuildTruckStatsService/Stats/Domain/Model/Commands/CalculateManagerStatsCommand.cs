namespace BuildTruckStatsService.Stats.Domain.Model.Commands;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public record CalculateManagerStatsCommand(
    int ManagerId,
    StatsPeriod Period,
    bool ForceRecalculation = false,
    bool SaveHistory = false,
    string? HistoryNotes = null)
{
    public bool IsValid() => ManagerId > 0 && Period != null;

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (ManagerId <= 0) errors.Add("ManagerId must be greater than 0");
        if (Period == null) errors.Add("Period is required");
        return errors;
    }

    public static CalculateManagerStatsCommand ForCurrentMonth(int managerId, bool saveHistory = false)
    {
        return new CalculateManagerStatsCommand(managerId, StatsPeriod.CurrentMonth(), false, saveHistory);
    }

    public static CalculateManagerStatsCommand ForCustomPeriod(int managerId, DateTime startDate, DateTime endDate,
        bool saveHistory = false, string? notes = null)
    {
        return new CalculateManagerStatsCommand(managerId, StatsPeriod.Custom(startDate, endDate), false, saveHistory, notes);
    }

    public static CalculateManagerStatsCommand WithForceRecalculation(int managerId, StatsPeriod period, bool saveHistory = true)
    {
        return new CalculateManagerStatsCommand(managerId, period, true, saveHistory, "Recálculo forzado");
    }
}
