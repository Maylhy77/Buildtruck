using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;
using BuildTruckIncidentService.Incidents.Domain.Repositories;
using BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckIncidentService.Incidents.Infrastructure.Persistence.EFC.Repositories;

public class IncidentRepository : BaseRepository<Incident, IncidentServiceDbContext>, IIncidentRepository
{
    public IncidentRepository(IncidentServiceDbContext context) : base(context) { }

    public async Task<IEnumerable<Incident>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Incident>()
            .Where(i => i.ProjectId == projectId)
            .ToListAsync();
    }
}
