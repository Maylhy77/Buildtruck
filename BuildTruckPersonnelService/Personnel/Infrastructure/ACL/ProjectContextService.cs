using System.Net.Http.Json;
using BuildTruckPersonnelService.Personnel.Application.ACL.Services;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.ACL;

public class ProjectContextService : IProjectContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private HttpClient Client => _httpClientFactory.CreateClient("ProjectService");

    public ProjectContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<ExistsResponse>($"/api/v1/projects/exists/{projectId}");
            return response?.Exists ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetProjectNameAsync(int projectId)
    {
        try
        {
            var project = await Client.GetFromJsonAsync<ProjectNameResponse>($"/api/v1/projects/{projectId}");
            return project?.Name;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UserCanAccessProjectAsync(int userId, int projectId)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<AccessResponse>(
                $"/api/v1/projects/user-access?userId={userId}&projectId={projectId}");
            return response?.HasAccess ?? false;
        }
        catch
        {
            return false;
        }
    }

    private record ExistsResponse(bool Exists);
    private record ProjectNameResponse(string Name);
    private record AccessResponse(bool HasAccess);
}
