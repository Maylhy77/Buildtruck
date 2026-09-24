namespace BuildTruckMachineryService.Machinery.Domain.Services;

using BuildTruckMachineryService.Machinery.Domain.Model.Aggregates;
using BuildTruckMachineryService.Machinery.Domain.Model.Queries;

public interface IMachineryQueryService
{
    Task<Machinery?> Handle(GetMachineryByIdQuery query);
    Task<IEnumerable<Machinery>> Handle(GetMachineryByProjectQuery query);
    Task<IEnumerable<Machinery>> Handle(GetActiveMachineryQuery query);
}