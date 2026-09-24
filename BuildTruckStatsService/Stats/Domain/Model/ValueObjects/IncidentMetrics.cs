namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class IncidentMetrics
{
    public int TotalIncidents { get; private set; }
    public int CriticalIncidents { get; private set; }
    public int OpenIncidents { get; private set; }
    public int ResolvedIncidents { get; private set; }
    public Dictionary<string, int> IncidentsBySeverity { get; private set; }
    public Dictionary<string, int> IncidentsByType { get; private set; }
    public Dictionary<string, int> IncidentsByStatus { get; private set; }
    public decimal AverageResolutionTimeHours { get; private set; }

    protected IncidentMetrics()
    {
        IncidentsBySeverity = new Dictionary<string, int>();
        IncidentsByType = new Dictionary<string, int>();
        IncidentsByStatus = new Dictionary<string, int>();
    }

    public IncidentMetrics(int totalIncidents, int criticalIncidents, int openIncidents, int resolvedIncidents,
        Dictionary<string, int>? incidentsBySeverity = null, Dictionary<string, int>? incidentsByType = null,
        Dictionary<string, int>? incidentsByStatus = null, decimal averageResolutionTimeHours = 0m)
    {
        TotalIncidents = Math.Max(0, totalIncidents);
        CriticalIncidents = Math.Max(0, criticalIncidents);
        OpenIncidents = Math.Max(0, openIncidents);
        ResolvedIncidents = Math.Max(0, resolvedIncidents);
        IncidentsBySeverity = incidentsBySeverity ?? new Dictionary<string, int>();
        IncidentsByType = incidentsByType ?? new Dictionary<string, int>();
        IncidentsByStatus = incidentsByStatus ?? new Dictionary<string, int>();
        AverageResolutionTimeHours = Math.Max(0m, averageResolutionTimeHours);
    }

    public static IncidentMetrics FromCounts(int total, int critical, int open, Dictionary<string, int>? bySeverity = null)
    {
        var resolved = total - open;
        var defaultSeverity = bySeverity ?? new Dictionary<string, int>
        {
            ["Alto"] = critical,
            ["Medio"] = Math.Max(0, total - critical - (total > critical ? (total - critical) / 2 : 0)),
            ["Bajo"] = Math.Max(0, total - critical - Math.Max(0, total - critical - (total - critical) / 2))
        };
        var defaultStatus = new Dictionary<string, int> { ["Reportado"] = open, ["Resuelto"] = resolved };
        return new IncidentMetrics(total, critical, open, resolved, defaultSeverity, null, defaultStatus);
    }

    public decimal GetCriticalRate()
    {
        if (TotalIncidents == 0) return 0m;
        return Math.Round((decimal)CriticalIncidents / TotalIncidents * 100, 2);
    }

    public decimal GetResolutionRate()
    {
        if (TotalIncidents == 0) return 0m;
        return Math.Round((decimal)ResolvedIncidents / TotalIncidents * 100, 2);
    }

    public decimal GetOpenRate()
    {
        if (TotalIncidents == 0) return 0m;
        return Math.Round((decimal)OpenIncidents / TotalIncidents * 100, 2);
    }

    public string GetSafetyStatus()
    {
        return CriticalIncidents switch
        {
            0 => "Excelente",
            <= 2 => "Bueno",
            <= 5 => "Regular",
            _ => "Crítico"
        };
    }

    public bool HasCriticalIncidents() => CriticalIncidents > 0;
    public bool NeedsAttention() => CriticalIncidents > 0 || GetOpenRate() > 20m;

    public string GetMostCommonSeverity()
    {
        if (!IncidentsBySeverity.Any()) return "Sin datos";
        return IncidentsBySeverity.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public string GetMostCommonType()
    {
        if (!IncidentsByType.Any()) return "Sin datos";
        return IncidentsByType.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public decimal GetSafetyScore()
    {
        if (TotalIncidents == 0) return 100m;
        var baseScore = 100m;
        baseScore -= CriticalIncidents * 20m;
        baseScore -= OpenIncidents * 5m;
        if (GetResolutionRate() > 80m) baseScore += 10m;
        return Math.Max(0m, Math.Min(100m, baseScore));
    }

    public string GetIncidentSummary()
    {
        if (TotalIncidents == 0) return "Sin incidentes";
        var summary = $"{TotalIncidents} incidentes";
        if (CriticalIncidents > 0) summary += $" ({CriticalIncidents} críticos)";
        if (OpenIncidents > 0) summary += $", {OpenIncidents} abiertos";
        return summary;
    }

    public string GetResolutionSummary()
    {
        if (AverageResolutionTimeHours == 0m) return "Sin datos de resolución";
        if (AverageResolutionTimeHours < 24m) return $"Resolución promedio: {AverageResolutionTimeHours:F1} horas";
        return $"Resolución promedio: {AverageResolutionTimeHours / 24m:F1} días";
    }

    public IncidentMetrics AddIncident(string severity, string type, string status)
    {
        var newBySeverity = new Dictionary<string, int>(IncidentsBySeverity);
        var newByType = new Dictionary<string, int>(IncidentsByType);
        var newByStatus = new Dictionary<string, int>(IncidentsByStatus);

        if (newBySeverity.ContainsKey(severity)) newBySeverity[severity]++; else newBySeverity[severity] = 1;
        if (newByType.ContainsKey(type)) newByType[type]++; else newByType[type] = 1;
        if (newByStatus.ContainsKey(status)) newByStatus[status]++; else newByStatus[status] = 1;

        var newCritical = severity.Equals("Alto", StringComparison.OrdinalIgnoreCase) ? CriticalIncidents + 1 : CriticalIncidents;
        var newOpen = status.Equals("Reportado", StringComparison.OrdinalIgnoreCase) || status.Equals("En Proceso", StringComparison.OrdinalIgnoreCase) ? OpenIncidents + 1 : OpenIncidents;
        var newResolved = status.Equals("Resuelto", StringComparison.OrdinalIgnoreCase) ? ResolvedIncidents + 1 : ResolvedIncidents;

        return new IncidentMetrics(TotalIncidents + 1, newCritical, newOpen, newResolved,
            newBySeverity, newByType, newByStatus, AverageResolutionTimeHours);
    }

    public override string ToString() => $"Incidents: {TotalIncidents} total ({CriticalIncidents} critical, {GetSafetyScore():F1}% safety score)";

    public override bool Equals(object? obj)
    {
        if (obj is not IncidentMetrics other) return false;
        return TotalIncidents == other.TotalIncidents && CriticalIncidents == other.CriticalIncidents &&
               OpenIncidents == other.OpenIncidents && ResolvedIncidents == other.ResolvedIncidents;
    }

    public override int GetHashCode() => HashCode.Combine(TotalIncidents, CriticalIncidents, OpenIncidents, ResolvedIncidents);
}
