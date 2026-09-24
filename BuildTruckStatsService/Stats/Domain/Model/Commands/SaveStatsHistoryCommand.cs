namespace BuildTruckStatsService.Stats.Domain.Model.Commands;

public record SaveStatsHistoryCommand(
    int ManagerStatsId,
    string? Notes = null,
    bool IsManualSnapshot = false,
    bool OverwriteExisting = false)
{
    public bool IsValid() => ManagerStatsId > 0;

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (ManagerStatsId <= 0) errors.Add("ManagerStatsId must be greater than 0");
        return errors;
    }

    public static SaveStatsHistoryCommand Automatic(int managerStatsId, string? notes = null)
    {
        return new SaveStatsHistoryCommand(managerStatsId, notes ?? "Snapshot automático", false, false);
    }

    public static SaveStatsHistoryCommand Manual(int managerStatsId, string notes)
    {
        return new SaveStatsHistoryCommand(managerStatsId, notes, true, false);
    }

    public static SaveStatsHistoryCommand WithOverwrite(int managerStatsId, string? notes = null)
    {
        return new SaveStatsHistoryCommand(managerStatsId, notes, false, true);
    }
}
