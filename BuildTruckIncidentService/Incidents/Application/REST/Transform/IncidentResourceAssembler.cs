using BuildTruckIncidentService.Incidents.Application.REST.Resources;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;

namespace BuildTruckIncidentService.Incidents.Application.REST.Transform;

public class IncidentResourceAssembler
{
    public static IncidentResource ToResource(Incident incident)
    {
        return new IncidentResource(
            incident.Id,
            incident.ProjectId,
            incident.Title,
            incident.Description,
            incident.IncidentType,
            incident.Severity,
            incident.Status,
            incident.Location,
            incident.ReportedBy,
            incident.AssignedTo,
            incident.OccurredAt,
            incident.ResolvedAt,
            incident.Image,
            incident.Notes,
            incident.UpdatedAt,
            incident.RegisterDate);
    }
}