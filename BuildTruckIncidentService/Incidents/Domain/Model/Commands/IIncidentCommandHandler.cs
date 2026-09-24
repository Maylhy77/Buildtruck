using System.Threading.Tasks;
namespace BuildTruckIncidentService.Incidents.Domain.Model.Commands;

public interface IIncidentCommandHandler
{
    Task<int> HandleAsync(CreateIncidentCommand command);
    Task HandleAsync(UpdateIncidentCommand command);
    Task DeleteAsync(int id);
    
    
}