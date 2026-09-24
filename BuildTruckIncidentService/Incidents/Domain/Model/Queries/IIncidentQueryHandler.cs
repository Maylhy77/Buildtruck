using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;

namespace BuildTruckIncidentService.Incidents.Domain.Model.Queries;

public interface IIncidentQueryHandler
{
    Task<Incident?> HandleAsync(GetIncidentByIdQuery query);
    Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query);
}