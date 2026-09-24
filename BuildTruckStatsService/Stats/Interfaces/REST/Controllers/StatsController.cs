namespace BuildTruckStatsService.Stats.Interfaces.REST.Controllers;

using BuildTruckStatsService.Stats.Domain.Model.Commands;
using BuildTruckStatsService.Stats.Domain.Model.Queries;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;
using BuildTruckStatsService.Stats.Domain.Services;
using BuildTruckStatsService.Stats.Interfaces.REST.Resources;
using BuildTruckStatsService.Stats.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Stats")]
public class StatsController : ControllerBase
{
    private readonly IStatsCommandService _commandService;
    private readonly IStatsQueryService _queryService;
    private readonly ILogger<StatsController> _logger;

    public StatsController(
        IStatsCommandService commandService,
        IStatsQueryService queryService,
        ILogger<StatsController> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("manager/{managerId:int}/current")]
    [SwaggerOperation(Summary = "Get current manager stats", OperationId = "GetCurrentManagerStats")]
    [SwaggerResponse(200, "Stats retrieved successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(404, "Manager not found or no stats available")]
    public async Task<ActionResult<ManagerStatsResource>> GetCurrentManagerStats(int managerId)
    {
        try
        {
            _logger.LogInformation("Getting current stats for manager {ManagerId}", managerId);
            var stats = await _queryService.GetCurrentStats(managerId);
            if (stats == null) return NotFound($"No current stats found for manager {managerId}");
            return Ok(StatsResourceAssembler.ToResourceFromEntity(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    [HttpGet("manager/{managerId:int}")]
    [SwaggerOperation(Summary = "Get manager stats for period", OperationId = "GetManagerStats")]
    [SwaggerResponse(200, "Stats retrieved successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(404, "Manager not found or no stats available")]
    public async Task<ActionResult<ManagerStatsResource>> GetManagerStats(
        int managerId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? periodType = null)
    {
        try
        {
            _logger.LogInformation("Getting stats for manager {ManagerId}", managerId);

            StatsPeriod? period = null;
            if (startDate.HasValue && endDate.HasValue)
                period = StatsPeriod.Custom(startDate.Value, endDate.Value);
            else if (!string.IsNullOrEmpty(periodType))
            {
                period = periodType.ToUpper() switch
                {
                    "CURRENT_MONTH" => StatsPeriod.CurrentMonth(),
                    "CURRENT_QUARTER" => StatsPeriod.CurrentQuarter(),
                    "CURRENT_YEAR" => StatsPeriod.CurrentYear(),
                    "LAST_30_DAYS" => StatsPeriod.LastNDays(30),
                    "LAST_90_DAYS" => StatsPeriod.LastNDays(90),
                    _ => StatsPeriod.CurrentMonth()
                };
            }

            var query = new GetManagerStatsQuery(managerId, period);
            var stats = await _queryService.Handle(query);
            if (stats == null) return NotFound($"No stats found for manager {managerId} in the specified period");
            return Ok(StatsResourceAssembler.ToResourceFromEntity(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    [HttpPost("manager/{managerId:int}/calculate")]
    [SwaggerOperation(Summary = "Calculate manager stats", OperationId = "CalculateManagerStats")]
    [SwaggerResponse(200, "Stats calculated successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(400, "Invalid request parameters")]
    [SwaggerResponse(404, "Manager not found")]
    public async Task<ActionResult<ManagerStatsResource>> CalculateManagerStats(
        int managerId,
        [FromBody] CalculateStatsResource? request = null)
    {
        try
        {
            _logger.LogInformation("Calculating stats for manager {ManagerId}", managerId);

            StatsPeriod period;
            if (request?.StartDate.HasValue == true && request?.EndDate.HasValue == true)
                period = StatsPeriod.Custom(request.StartDate.Value, request.EndDate.Value);
            else
                period = StatsPeriod.CurrentMonth();

            var command = new CalculateManagerStatsCommand(
                managerId,
                period,
                request?.ForceRecalculation ?? false,
                request?.SaveHistory ?? true,
                request?.Notes);

            var stats = await _commandService.Handle(command);
            return Ok(StatsResourceAssembler.ToResourceFromEntity(stats));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for calculating stats for manager {ManagerId}", managerId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    [HttpGet("manager/{managerId:int}/history")]
    [SwaggerOperation(Summary = "Get manager stats history", OperationId = "GetManagerStatsHistory")]
    [SwaggerResponse(200, "History retrieved successfully", typeof(IEnumerable<StatsHistoryResource>))]
    [SwaggerResponse(404, "Manager not found")]
    public async Task<ActionResult<IEnumerable<StatsHistoryResource>>> GetManagerStatsHistory(
        int managerId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? periodType = null,
        [FromQuery] int? limit = null,
        [FromQuery] bool includeManual = true)
    {
        try
        {
            _logger.LogInformation("Getting stats history for manager {ManagerId}", managerId);

            var query = new GetStatsHistoryQuery(managerId, startDate, endDate, periodType, limit, true, includeManual);
            var history = await _queryService.Handle(query);
            return Ok(history.Select(StatsResourceAssembler.ToResourceFromEntity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats history for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    [HttpGet("manager/{managerId:int}/dashboard")]
    [SwaggerOperation(Summary = "Get manager dashboard", OperationId = "GetManagerDashboard")]
    [SwaggerResponse(200, "Dashboard data retrieved successfully")]
    [SwaggerResponse(404, "Manager not found")]
    public async Task<ActionResult<object>> GetManagerDashboard(int managerId)
    {
        try
        {
            _logger.LogInformation("Getting dashboard data for manager {ManagerId}", managerId);
            var dashboard = await _queryService.GetManagerDashboard(managerId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }
}
