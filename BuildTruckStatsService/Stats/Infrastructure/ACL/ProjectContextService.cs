namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class ProjectContextService : IProjectContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProjectContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("ProjectService");

    public ProjectContextService(IHttpClientFactory httpClientFactory, ILogger<ProjectContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectMetrics> GetProjectMetricsAsync(int managerId, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting project metrics for manager {ManagerId}", managerId);

            var projects = await GetProjectsByManagerAsync(managerId);

            var filteredProjects = projects.Where(p =>
                !p.StartDate.HasValue || period.Contains(p.StartDate.Value)).ToList();

            var totalProjects = filteredProjects.Count;
            var activeProjects = filteredProjects.Count(p => IsActiveStatus(p.State));
            var completedProjects = filteredProjects.Count(p => IsCompletedStatus(p.State));
            var plannedProjects = filteredProjects.Count(p => IsPlannedStatus(p.State));
            var overdueProjects = filteredProjects.Count(p =>
                p.StartDate.HasValue && p.StartDate.Value < DateTime.Now && !IsCompletedStatus(p.State));

            var projectsByStatus = filteredProjects
                .Where(p => !string.IsNullOrEmpty(p.State))
                .GroupBy(p => p.State!)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ProjectMetrics(totalProjects, activeProjects, completedProjects, plannedProjects, overdueProjects, projectsByStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project metrics for manager {ManagerId}", managerId);
            return ProjectMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetProjectsByStatusAsync(int managerId, StatsPeriod period)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            return projects
                .Where(p => !string.IsNullOrEmpty(p.State) && (!p.StartDate.HasValue || period.Contains(p.StartDate.Value)))
                .GroupBy(p => p.State!)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by status for manager {ManagerId}", managerId);
            return new Dictionary<string, int>();
        }
    }

    public async Task<bool> ManagerHasProjectsAsync(int managerId)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            return projects.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if manager {ManagerId} has projects", managerId);
            return false;
        }
    }

    public async Task<int> GetActiveProjectsCountAsync(int managerId)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            return projects.Count(p => IsActiveStatus(p.State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<int> GetCompletedProjectsCountAsync(int managerId, StatsPeriod period)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            return projects.Count(p => IsCompletedStatus(p.State) && (!p.StartDate.HasValue || period.Contains(p.StartDate.Value)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<int> GetOverdueProjectsCountAsync(int managerId)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            var now = DateTime.Now;
            return projects.Count(p => p.StartDate.HasValue && p.StartDate.Value < now && !IsCompletedStatus(p.State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<List<int>> GetManagerProjectIdsAsync(int managerId)
    {
        try
        {
            var projects = await GetProjectsByManagerAsync(managerId);
            return projects.Select(p => p.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project IDs for manager {ManagerId}", managerId);
            return new List<int>();
        }
    }

    private async Task<List<ProjectDto>> GetProjectsByManagerAsync(int managerId)
    {
        var result = await Client.GetFromJsonAsync<List<ProjectDto>>($"/api/v1/projects?managerId={managerId}");
        return result ?? new List<ProjectDto>();
    }

    private static bool IsActiveStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        var n = state.ToLowerInvariant();
        return n is "activo" or "active" or "en_progreso" or "in_progress" or "iniciado" or "started";
    }

    private static bool IsCompletedStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        var n = state.ToLowerInvariant();
        return n is "completado" or "completed" or "finalizado" or "finished" or "terminado" or "done";
    }

    private static bool IsPlannedStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        var n = state.ToLowerInvariant();
        return n is "planificado" or "planned" or "programado" or "scheduled" or "pendiente" or "pending";
    }

    private record ProjectDto(int Id, string? State, DateTime? StartDate);
}
