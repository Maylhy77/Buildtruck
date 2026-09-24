using System;
using BuildTruckIncidentService.Incidents.Domain.ValueObjects;

namespace BuildTruckIncidentService.Incidents.Domain.Aggregates;

public class Incident
{
    public int Id { get; set; }
    public int? ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string IncidentType { get; set; } = "";
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medio;
    public IncidentStatus Status { get; set; } = IncidentStatus.Reportado;
    public string Location { get; set; } = "";
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow.AddHours(-5); // Peru timezone
    public DateTime? ResolvedAt { get; set; }
    public string? Image { get; set; }
    public string Notes { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(-5);
    public DateTime RegisterDate { get; set; } = DateTime.UtcNow.AddHours(-5);
}