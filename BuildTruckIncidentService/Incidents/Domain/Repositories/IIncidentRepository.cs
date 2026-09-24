using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckIncidentService.Incidents.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<IEnumerable<Incident>> FindByProjectIdAsync(int projectId);
}