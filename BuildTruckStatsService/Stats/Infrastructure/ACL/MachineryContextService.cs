namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class MachineryContextService : IMachineryContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MachineryContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("MachineryService");

    public MachineryContextService(IHttpClientFactory httpClientFactory, ILogger<MachineryContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MachineryMetrics> GetMachineryMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return MachineryMetrics.FromCounts(0, 0, 0);

            var allMachinery = await GetMachineryForProjectsAsync(projectIds);

            var total = allMachinery.Count;
            var active = allMachinery.Count(m => IsActiveStatus(m.Status));
            var inMaintenance = allMachinery.Count(m => IsMaintenanceStatus(m.Status));
            var inactive = allMachinery.Count(m => IsInactiveStatus(m.Status));

            var byStatus = allMachinery
                .Where(m => !string.IsNullOrEmpty(m.Status))
                .GroupBy(m => m.Status!)
                .ToDictionary(g => g.Key, g => g.Count());

            var byType = allMachinery
                .Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!)
                .ToDictionary(g => g.Key, g => g.Count());

            var byProject = allMachinery
                .GroupBy(m => m.ProjectId)
                .ToDictionary(g => $"Project {g.Key}", g => g.Count());

            var availabilityRate = total > 0
                ? Math.Round((decimal)active / total * 100, 2)
                : 0m;

            _logger.LogDebug("Machinery metrics: {Total} total, {Active} active, {Maintenance} in maintenance",
                total, active, inMaintenance);

            return new MachineryMetrics(total, active, inMaintenance, inactive, byStatus, byType, byProject, availabilityRate, 0m);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery metrics for projects");
            return MachineryMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetMachineryByStatusAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Where(m => !string.IsNullOrEmpty(m.Status))
                .GroupBy(m => m.Status!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by status");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetMachineryByTypeAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by type");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetTotalMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total machinery count");
            return 0;
        }
    }

    public async Task<int> GetActiveMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Count(m => IsActiveStatus(m.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active machinery count");
            return 0;
        }
    }

    public async Task<int> GetMachineryInMaintenanceCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Count(m => IsMaintenanceStatus(m.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery in maintenance count");
            return 0;
        }
    }

    public async Task<int> GetInactiveMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.Count(m => IsInactiveStatus(m.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inactive machinery count");
            return 0;
        }
    }

    public async Task<decimal> GetOverallAvailabilityRateAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0m;
            var all = await GetMachineryForProjectsAsync(projectIds);
            if (!all.Any()) return 0m;
            return Math.Round((decimal)all.Count(m => IsActiveStatus(m.Status)) / all.Count * 100, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overall availability rate");
            return 0m;
        }
    }

    public async Task<decimal> GetAverageMaintenanceTimeAsync(List<int> projectIds, StatsPeriod period)
    {
        _logger.LogDebug("Getting average maintenance time for {Count} projects (placeholder)", projectIds.Count);
        return 0m;
    }

    public async Task<Dictionary<string, int>> GetMachineryByProjectAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var all = await GetMachineryForProjectsAsync(projectIds);
            return all.GroupBy(m => m.ProjectId)
                .ToDictionary(g => $"Project {g.Key}", g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by project");
            return new Dictionary<string, int>();
        }
    }

    private async Task<List<MachineryDto>> GetMachineryForProjectsAsync(List<int> projectIds)
    {
        var all = new List<MachineryDto>();
        foreach (var projectId in projectIds)
        {
            try
            {
                var result = await Client.GetFromJsonAsync<List<MachineryDto>>($"/api/v1/machinery?projectId={projectId}");
                if (result != null) all.AddRange(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
            }
        }
        return all;
    }

    private static bool IsActiveStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var n = status.ToLowerInvariant();
        return n is "active" or "activo" or "operational" or "operacional";
    }

    private static bool IsMaintenanceStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var n = status.ToLowerInvariant();
        return n is "maintenance" or "mantenimiento" or "repair" or "reparacion";
    }

    private static bool IsInactiveStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return true;
        var n = status.ToLowerInvariant();
        return n is "inactive" or "inactivo" or "offline" or "down" or "broken" or "fuera_de_servicio";
    }

    private record MachineryDto(int Id, int ProjectId, string? Status, string? Type);
}
