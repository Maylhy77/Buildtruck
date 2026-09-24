namespace BuildTruckStatsService.Stats.Domain.Model.Aggregates;

using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

public partial class StatsHistory : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }

    public static DateTime GetCurrentPeruTime() => DateTime.UtcNow.AddHours(-5);

    public bool IsFromToday()
    {
        var today = GetCurrentPeruTime().Date;
        return SnapshotDate.Date == today;
    }

    public bool IsFromThisWeek()
    {
        var now = GetCurrentPeruTime();
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return SnapshotDate.Date >= startOfWeek && SnapshotDate.Date < endOfWeek;
    }

    public int GetAgeInDays() => (GetCurrentPeruTime().Date - SnapshotDate.Date).Days;

    public string GetSnapshotDateFormatted() => SnapshotDate.ToString("dd/MM/yyyy HH:mm");

    public string GetRelativeSnapshotTime()
    {
        var timeSpan = GetCurrentPeruTime() - SnapshotDate;
        return timeSpan.TotalDays switch
        {
            < 1 => "Hoy",
            < 2 => "Ayer",
            < 7 => $"Hace {(int)timeSpan.TotalDays} días",
            < 30 => $"Hace {(int)(timeSpan.TotalDays / 7)} semana(s)",
            < 365 => $"Hace {(int)(timeSpan.TotalDays / 30)} mes(es)",
            _ => $"Hace {(int)(timeSpan.TotalDays / 365)} año(s)"
        };
    }

    public bool ShouldBeArchived() => GetAgeInDays() > 365;

    public void UpdateNotesWithTimestamp(string newNotes)
    {
        var timestamp = GetCurrentPeruTime().ToString("dd/MM/yyyy HH:mm");
        Notes = string.IsNullOrEmpty(Notes)
            ? $"[{timestamp}] {newNotes}"
            : $"{Notes}\n[{timestamp}] {newNotes}";
        UpdatedDate = DateTimeOffset.UtcNow.AddHours(-5);
    }
}
