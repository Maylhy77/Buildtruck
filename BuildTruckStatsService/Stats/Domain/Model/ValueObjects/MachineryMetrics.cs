namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class MachineryMetrics
{
    public int TotalMachinery { get; private set; }
    public int ActiveMachinery { get; private set; }
    public int InMaintenanceMachinery { get; private set; }
    public int InactiveMachinery { get; private set; }
    public Dictionary<string, int> MachineryByStatus { get; private set; }
    public Dictionary<string, int> MachineryByType { get; private set; }
    public Dictionary<string, int> MachineryByProject { get; private set; }
    public decimal OverallAvailabilityRate { get; private set; }
    public decimal AverageMaintenanceTimeHours { get; private set; }

    protected MachineryMetrics()
    {
        MachineryByStatus = new Dictionary<string, int>();
        MachineryByType = new Dictionary<string, int>();
        MachineryByProject = new Dictionary<string, int>();
    }

    public MachineryMetrics(int totalMachinery, int activeMachinery, int inMaintenanceMachinery, int inactiveMachinery,
        Dictionary<string, int>? machineryByStatus = null, Dictionary<string, int>? machineryByType = null,
        Dictionary<string, int>? machineryByProject = null, decimal overallAvailabilityRate = 0m,
        decimal averageMaintenanceTimeHours = 0m)
    {
        TotalMachinery = Math.Max(0, totalMachinery);
        ActiveMachinery = Math.Max(0, activeMachinery);
        InMaintenanceMachinery = Math.Max(0, inMaintenanceMachinery);
        InactiveMachinery = Math.Max(0, inactiveMachinery);
        MachineryByStatus = machineryByStatus ?? new Dictionary<string, int>();
        MachineryByType = machineryByType ?? new Dictionary<string, int>();
        MachineryByProject = machineryByProject ?? new Dictionary<string, int>();
        OverallAvailabilityRate = Math.Max(0m, Math.Min(100m, overallAvailabilityRate));
        AverageMaintenanceTimeHours = Math.Max(0m, averageMaintenanceTimeHours);
    }

    public static MachineryMetrics FromCounts(int total, int active, int maintenance,
        Dictionary<string, int>? byStatus = null, Dictionary<string, int>? byType = null)
    {
        var inactive = Math.Max(0, total - active - maintenance);
        var defaultStatus = byStatus ?? new Dictionary<string, int>
        {
            ["active"] = active, ["maintenance"] = maintenance, ["inactive"] = inactive
        };
        return new MachineryMetrics(total, active, maintenance, inactive, defaultStatus, byType);
    }

    public decimal GetActiveRate()
    {
        if (TotalMachinery == 0) return 0m;
        return Math.Round((decimal)ActiveMachinery / TotalMachinery * 100, 2);
    }

    public decimal GetMaintenanceRate()
    {
        if (TotalMachinery == 0) return 0m;
        return Math.Round((decimal)InMaintenanceMachinery / TotalMachinery * 100, 2);
    }

    public decimal GetInactiveRate()
    {
        if (TotalMachinery == 0) return 0m;
        return Math.Round((decimal)InactiveMachinery / TotalMachinery * 100, 2);
    }

    public decimal GetOperationalRate()
    {
        if (TotalMachinery == 0) return 0m;
        return Math.Round((decimal)(ActiveMachinery + InMaintenanceMachinery) / TotalMachinery * 100, 2);
    }

    public string GetAvailabilityStatus()
    {
        return GetActiveRate() switch
        {
            >= 90m => "Excelente",
            >= 80m => "Bueno",
            >= 60m => "Regular",
            >= 40m => "Bajo",
            _ => "Crítico"
        };
    }

    public bool HasHighAvailability() => GetActiveRate() >= 80m;
    public bool NeedsMaintenance() => InMaintenanceMachinery > TotalMachinery * 0.3m;

    public string GetMostCommonStatus()
    {
        if (!MachineryByStatus.Any()) return "Sin datos";
        return MachineryByStatus.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public string GetMostCommonType()
    {
        if (!MachineryByType.Any()) return "Sin datos";
        return MachineryByType.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public string GetProjectWithMostMachinery()
    {
        if (!MachineryByProject.Any()) return "Sin datos";
        return MachineryByProject.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public decimal GetEfficiencyScore()
    {
        if (TotalMachinery == 0) return 100m;
        var baseScore = GetActiveRate();
        if (GetMaintenanceRate() > 20m) baseScore -= (GetMaintenanceRate() - 20m) * 0.5m;
        baseScore -= GetInactiveRate() * 1.5m;
        if (OverallAvailabilityRate > 85m) baseScore += 10m;
        return Math.Max(0m, Math.Min(100m, baseScore));
    }

    public string GetMachinerySummary()
    {
        if (TotalMachinery == 0) return "Sin maquinaria";
        var summary = $"{TotalMachinery} máquinas ({ActiveMachinery} activas)";
        if (InMaintenanceMachinery > 0) summary += $", {InMaintenanceMachinery} en mantenimiento";
        if (InactiveMachinery > 0) summary += $", {InactiveMachinery} inactivas";
        return summary;
    }

    public string GetMaintenanceSummary()
    {
        if (AverageMaintenanceTimeHours == 0m) return "Sin datos de mantenimiento";
        if (AverageMaintenanceTimeHours < 24m) return $"Mantenimiento promedio: {AverageMaintenanceTimeHours:F1} horas";
        return $"Mantenimiento promedio: {AverageMaintenanceTimeHours / 24m:F1} días";
    }

    public List<string> GetMaintenanceAlerts()
    {
        var alerts = new List<string>();
        if (GetMaintenanceRate() > 30m) alerts.Add("Alto porcentaje de maquinaria en mantenimiento");
        if (GetInactiveRate() > 15m) alerts.Add("Alto porcentaje de maquinaria inactiva");
        if (GetActiveRate() < 60m) alerts.Add("Baja disponibilidad de maquinaria activa");
        if (AverageMaintenanceTimeHours > 72m) alerts.Add("Tiempo de mantenimiento promedio elevado");
        return alerts;
    }

    public MachineryMetrics AddMachinery(string status, string type, string project)
    {
        var newByStatus = new Dictionary<string, int>(MachineryByStatus);
        var newByType = new Dictionary<string, int>(MachineryByType);
        var newByProject = new Dictionary<string, int>(MachineryByProject);

        if (newByStatus.ContainsKey(status)) newByStatus[status]++; else newByStatus[status] = 1;
        if (newByType.ContainsKey(type)) newByType[type]++; else newByType[type] = 1;
        if (newByProject.ContainsKey(project)) newByProject[project]++; else newByProject[project] = 1;

        var newActive = status.Equals("active", StringComparison.OrdinalIgnoreCase) ? ActiveMachinery + 1 : ActiveMachinery;
        var newMaintenance = status.Equals("maintenance", StringComparison.OrdinalIgnoreCase) ? InMaintenanceMachinery + 1 : InMaintenanceMachinery;
        var newInactive = status.Equals("inactive", StringComparison.OrdinalIgnoreCase) ? InactiveMachinery + 1 : InactiveMachinery;

        return new MachineryMetrics(TotalMachinery + 1, newActive, newMaintenance, newInactive,
            newByStatus, newByType, newByProject, OverallAvailabilityRate, AverageMaintenanceTimeHours);
    }

    public MachineryMetrics UpdateAvailability(decimal newAvailabilityRate)
    {
        return new MachineryMetrics(TotalMachinery, ActiveMachinery, InMaintenanceMachinery, InactiveMachinery,
            MachineryByStatus, MachineryByType, MachineryByProject,
            Math.Max(0m, Math.Min(100m, newAvailabilityRate)), AverageMaintenanceTimeHours);
    }

    public override string ToString() => $"Machinery: {ActiveMachinery}/{TotalMachinery} active ({GetActiveRate()}% availability)";

    public override bool Equals(object? obj)
    {
        if (obj is not MachineryMetrics other) return false;
        return TotalMachinery == other.TotalMachinery && ActiveMachinery == other.ActiveMachinery &&
               InMaintenanceMachinery == other.InMaintenanceMachinery && InactiveMachinery == other.InactiveMachinery;
    }

    public override int GetHashCode() => HashCode.Combine(TotalMachinery, ActiveMachinery, InMaintenanceMachinery, InactiveMachinery);
}
