using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record ProjectDto(int Id, string Name, string Status, int? ManagerId, List<int>? MemberIds);

public class ProjectContextService : IProjectContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProjectContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<int>> GetAllActiveProjectsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProjectService");
            var response = await client.GetAsync("/api/v1/projects?status=active");
            if (!response.IsSuccessStatusCode) return [];
            var projects = await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>(JsonOptions);
            return projects?.Select(p => p.Id) ?? [];
        }
        catch { return []; }
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProjectService");
            var response = await client.GetAsync($"/api/v1/projects/{projectId}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<string> GetProjectNameAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProjectService");
            var response = await client.GetAsync($"/api/v1/projects/{projectId}");
            if (!response.IsSuccessStatusCode) return string.Empty;
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);
            return project?.Name ?? string.Empty;
        }
        catch { return string.Empty; }
    }

    public async Task<int> GetProjectManagerIdAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProjectService");
            var response = await client.GetAsync($"/api/v1/projects/{projectId}");
            if (!response.IsSuccessStatusCode) return 0;
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);
            return project?.ManagerId ?? 0;
        }
        catch { return 0; }
    }

    public async Task<IEnumerable<int>> GetProjectMemberIdsAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProjectService");
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/members");
            if (!response.IsSuccessStatusCode) return [];
            var ids = await response.Content.ReadFromJsonAsync<IEnumerable<int>>(JsonOptions);
            return ids ?? [];
        }
        catch { return []; }
    }
}
