namespace BuildTruckIncidentService.Incidents.Domain.ValueObjects;

public enum IncidentStatus
{
    Reportado,
    InProgress,
    Resolved
}

public static class IncidentStatusExtensions
{
    public static IncidentStatus FromString(string value)
    {
        return Enum.TryParse<IncidentStatus>(value, true, out var result)
            ? result
            : throw new ArgumentException($"Valor inválido para IncidentStatus: {value}");
    }
}