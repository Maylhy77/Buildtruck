using BuildTruckMachineryService.Machinery.Domain.Model.Queries;
using BuildTruckMachineryService.Machinery.Domain.Services;

namespace BuildTruckMachineryService.Machinery.Application.Internal.QueryServices;

public class MachineryQueryService : IMachineryQueryService
{
    private readonly GetActiveMachineryQueryHandler _activeMachineryQueryHandler;
    private readonly GetMachineryByIdQueryHandler _machineryByIdQueryHandler;
    private readonly GetMachineryByProjectQueryHandler _machineryByProjectQueryHandler;
    private readonly ILogger<MachineryQueryService> _logger;

    public MachineryQueryService(
        GetActiveMachineryQueryHandler activeMachineryQueryHandler,
        GetMachineryByIdQueryHandler machineryByIdQueryHandler,
        GetMachineryByProjectQueryHandler machineryByProjectQueryHandler,
        ILogger<MachineryQueryService> logger)
    {
        _activeMachineryQueryHandler = activeMachineryQueryHandler;
        _machineryByIdQueryHandler = machineryByIdQueryHandler;
        _machineryByProjectQueryHandler = machineryByProjectQueryHandler;
        _logger = logger;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetActiveMachineryQuery query)
    {
        _logger.LogInformation("Handling GetActiveMachineryQuery for ProjectId {ProjectId}", query.ProjectId);
        var result = await _activeMachineryQueryHandler.Handle(query);
        _logger.LogInformation("Retrieved {Count} active machinery for ProjectId {ProjectId}", result.Count(), query.ProjectId);
        return result;
    }

    public async Task<Domain.Model.Aggregates.Machinery?> Handle(GetMachineryByIdQuery query)
    {
        _logger.LogInformation("Handling GetMachineryByIdQuery for Id {Id}", query.Id);
        var result = await _machineryByIdQueryHandler.Handle(query);
        if (result == null)
            _logger.LogWarning("No machinery found for Id {Id}", query.Id);
        return result;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetMachineryByProjectQuery query)
    {
        _logger.LogInformation("Handling GetMachineryByProjectQuery for ProjectId {ProjectId}", query.ProjectId);
        var result = await _machineryByProjectQueryHandler.Handle(query);
        _logger.LogInformation("Retrieved {Count} machinery for ProjectId {ProjectId}", result.Count(), query.ProjectId);
        return result;
    }
}