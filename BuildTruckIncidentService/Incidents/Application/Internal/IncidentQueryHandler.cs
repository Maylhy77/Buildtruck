using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;
using BuildTruckIncidentService.Incidents.Domain.Model.Queries;
using BuildTruckIncidentService.Incidents.Domain.Repositories;

namespace BuildTruckIncidentService.Incidents.Application.Internal
{
    public class IncidentQueryHandler : IIncidentQueryHandler
    {
        private readonly IIncidentRepository _incidentRepository;

        // ✅ Agregar constructor con dependency injection
        public IncidentQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<Incident?> HandleAsync(GetIncidentByIdQuery query)
        {
            // ✅ Usar el repository real, no devolver null
            return await _incidentRepository.FindByIdAsync(query.Id);
        }

        public async Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query)
        {
            // ✅ Usar el repository real, no devolver lista vacía
            return await _incidentRepository.FindByProjectIdAsync(query.ProjectId);
        }
    }
}