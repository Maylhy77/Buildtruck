using BuildTruckMachineryService.Machinery.Domain.Model.Aggregates;
using BuildTruckMachineryService.Machinery.Domain.Model.Queries;
using BuildTruckMachineryService.Machinery.Domain.Repositories;

namespace BuildTruckMachineryService.Machinery.Application.Internal.QueryServices;

public class GetMachineryByIdQueryHandler
{
    private readonly IMachineryRepository _machineryRepository;

    public GetMachineryByIdQueryHandler(IMachineryRepository machineryRepository)
    {
        _machineryRepository = machineryRepository;
    }

    public async Task<Domain.Model.Aggregates.Machinery?> Handle(GetMachineryByIdQuery query)
    {
        return await _machineryRepository.FindByIdAsync(query.Id);
    }
}