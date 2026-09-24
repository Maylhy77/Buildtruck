using BuildTruckMachineryService.Machinery.Domain.Model.Aggregates;
using BuildTruckMachineryService.Machinery.Domain.Model.Queries;
using BuildTruckMachineryService.Machinery.Domain.Model.ValueObjects;
using BuildTruckMachineryService.Machinery.Domain.Repositories;

namespace BuildTruckMachineryService.Machinery.Application.Internal.QueryServices;

public class GetActiveMachineryQueryHandler
{
    private readonly IMachineryRepository _machineryRepository;

    public GetActiveMachineryQueryHandler(IMachineryRepository machineryRepository)
    {
        _machineryRepository = machineryRepository;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetActiveMachineryQuery query)
    {
        var machinery = await _machineryRepository.FindByProjectIdAsync(query.ProjectId);
        return machinery.Where(m => m.Status == MachineryStatus.Active.ToString());
    }
}