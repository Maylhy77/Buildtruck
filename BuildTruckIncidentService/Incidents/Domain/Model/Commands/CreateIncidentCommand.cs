namespace BuildTruckIncidentService.Incidents.Domain.Model.Commands;

public record CreateIncidentCommand(
    int? ProjectId,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    string Location,
    string? ReportedBy,
    string? AssignedTo,
    DateTime OccurredAt,
    string? Image,
    string Notes,
    string? ImagePath // <-- Agrega esta línea
);