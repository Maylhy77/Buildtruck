namespace BuildTruckIncidentService.Incidents.Domain.ValueObjects;

public enum IncidentSeverity
{
    Low,
    Medio,
    High
}

public static class IncidentSeverityExtensions
{
    public static IncidentSeverity FromString(string value)
    {
        return Enum.TryParse<IncidentSeverity>(value, true, out var result)
            ? result
            : throw new ArgumentException($"Valor inválido para IncidentSeverity: {value}");
    }
}