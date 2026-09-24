namespace BuildTruckStatsService.Stats.Domain.Model.Aggregates;

using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

public partial class ManagerStats : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }

    public static DateTime GetCurrentPeruTime() => DateTime.UtcNow.AddHours(-5);

    public void UpdateTimestamps()
    {
        UpdatedDate = DateTimeOffset.UtcNow.AddHours(-5);
        CalculatedAt = GetCurrentPeruTime();
    }

    public bool IsRecent()
    {
        var hourAgo = GetCurrentPeruTime().AddHours(-1);
        return CalculatedAt >= hourAgo;
    }

    public bool IsOutdated()
    {
        var dayAgo = GetCurrentPeruTime().AddDays(-1);
        return CalculatedAt < dayAgo;
    }

    public double GetAgeInHours() => (GetCurrentPeruTime() - CalculatedAt).TotalHours;

    public string GetCalculatedTimeFormatted() => CalculatedAt.ToString("dd/MM/yyyy HH:mm:ss");

    public string GetRelativeCalculationTime()
    {
        var timeSpan = GetCurrentPeruTime() - CalculatedAt;
        return timeSpan.TotalMinutes switch
        {
            < 1 => "Hace menos de un minuto",
            < 60 => $"Hace {(int)timeSpan.TotalMinutes} minuto(s)",
            < 1440 => $"Hace {(int)timeSpan.TotalHours} hora(s)",
            _ => $"Hace {(int)timeSpan.TotalDays} día(s)"
        };
    }
}
