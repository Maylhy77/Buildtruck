namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

/// <summary>
/// HTTP ACL implementation for the Incident bounded context.
/// This implementation was missing from the monolith — it calls the IncidentService
/// via HTTP at GET /api/v1/incidents?projectId={id}.
/// </summary>
public class IncidentContextService : IIncidentContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IncidentContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("IncidentService");

    public IncidentContextService(IHttpClientFactory httpClientFactory, ILogger<IncidentContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncidentMetrics> GetIncidentMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return IncidentMetrics.FromCounts(0, 0, 0);

            var all = await GetIncidentsForProjectsAsync(projectIds, period);

            var total = all.Count;
            var critical = all.Count(i => IsCriticalSeverity(i.Severity));
            var open = all.Count(i => IsOpenStatus(i.Status));
            var resolved = all.Count(i => IsResolvedStatus(i.Status));

            var bySeverity = all.Where(i => !string.IsNullOrEmpty(i.Severity))
                .GroupBy(i => i.Severity!).ToDictionary(g => g.Key, g => g.Count());

            var byType = all.Where(i => !string.IsNullOrEmpty(i.Type))
                .GroupBy(i => i.Type!).ToDictionary(g => g.Key, g => g.Count());

            var byStatus = all.Where(i => !string.IsNullOrEmpty(i.Status))
                .GroupBy(i => i.Status!).ToDictionary(g => g.Key, g => g.Count());

            decimal avgResolutionHours = 0m;
            var resolved2 = all.Where(i => IsResolvedStatus(i.Status) && i.ResolvedAt.HasValue && i.ReportedAt.HasValue).ToList();
            if (resolved2.Any())
            {
                avgResolutionHours = (decimal)resolved2
                    .Average(i => (i.ResolvedAt!.Value - i.ReportedAt!.Value).TotalHours);
            }

            _logger.LogDebug("Incident metrics: {Total} total, {Critical} critical, {Open} open", total, critical, open);

            return new IncidentMetrics(total, critical, open, resolved, bySeverity, byType, byStatus, avgResolutionHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incident metrics for projects");
            return IncidentMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsBySeverityAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Where(i => !string.IsNullOrEmpty(i.Severity))
                .GroupBy(i => i.Severity!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by severity");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsByTypeAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Where(i => !string.IsNullOrEmpty(i.Type))
                .GroupBy(i => i.Type!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by type");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsByStatusAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Where(i => !string.IsNullOrEmpty(i.Status))
                .GroupBy(i => i.Status!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by status");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetTotalIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total incidents count");
            return 0;
        }
    }

    public async Task<int> GetCriticalIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Count(i => IsCriticalSeverity(i.Severity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical incidents count");
            return 0;
        }
    }

    public async Task<int> GetOpenIncidentsCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            // For open incidents, no period filter
            var all = await GetIncidentsForProjectsAsync(projectIds, null);
            return all.Count(i => IsOpenStatus(i.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting open incidents count");
            return 0;
        }
    }

    public async Task<int> GetResolvedIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            return all.Count(i => IsResolvedStatus(i.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resolved incidents count");
            return 0;
        }
    }

    public async Task<decimal> GetAverageResolutionTimeAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0m;
            var all = await GetIncidentsForProjectsAsync(projectIds, period);
            var resolved = all.Where(i => IsResolvedStatus(i.Status) && i.ResolvedAt.HasValue && i.ReportedAt.HasValue).ToList();
            if (!resolved.Any()) return 0m;
            return (decimal)resolved.Average(i => (i.ResolvedAt!.Value - i.ReportedAt!.Value).TotalHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average resolution time");
            return 0m;
        }
    }

    private async Task<List<IncidentDto>> GetIncidentsForProjectsAsync(List<int> projectIds, StatsPeriod? period)
    {
        var all = new List<IncidentDto>();
        foreach (var projectId in projectIds)
        {
            try
            {
                var result = await Client.GetFromJsonAsync<List<IncidentDto>>($"/api/v1/incidents?projectId={projectId}");
                if (result != null)
                {
                    // Filter by period if provided
                    var filtered = period != null
                        ? result.Where(i => !i.ReportedAt.HasValue || period.Contains(i.ReportedAt.Value)).ToList()
                        : result;
                    all.AddRange(filtered);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
            }
        }
        return all;
    }

    private static bool IsCriticalSeverity(string? severity)
    {
        if (string.IsNullOrEmpty(severity)) return false;
        var n = severity.ToLowerInvariant();
        return n is "alto" or "high" or "crítico" or "critico" or "critical";
    }

    private static bool IsOpenStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var n = status.ToLowerInvariant();
        return n is "reportado" or "reported" or "en proceso" or "in_process" or "abierto" or "open" or "pendiente" or "pending";
    }

    private static bool IsResolvedStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var n = status.ToLowerInvariant();
        return n is "resuelto" or "resolved" or "cerrado" or "closed" or "completado" or "completed";
    }

    private record IncidentDto(
        int Id,
        int ProjectId,
        string? Severity,
        string? Type,
        string? Status,
        DateTime? ReportedAt,
        DateTime? ResolvedAt);
}
