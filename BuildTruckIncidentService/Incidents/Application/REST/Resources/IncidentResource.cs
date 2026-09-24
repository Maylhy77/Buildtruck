using System;
using BuildTruckIncidentService.Incidents.Domain.ValueObjects;

namespace BuildTruckIncidentService.Incidents.Application.REST.Resources;

public record IncidentResource(
    int Id,
    int? ProjectId,
    string Title,
    string Description,
    string IncidentType,
    IncidentSeverity Severity,
    IncidentStatus Status,
    string Location,
    string? ReportedBy,
    string? AssignedTo,
    DateTime OccurredAt,
    DateTime? ResolvedAt,
    string? Image,
    string Notes,
    DateTime UpdatedAt,
    DateTime RegisterDate);