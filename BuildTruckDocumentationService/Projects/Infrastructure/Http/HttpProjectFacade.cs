using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckDocumentationService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckDocumentationService.Projects.Infrastructure.Http;

public class HttpProjectFacade(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<HttpProjectFacade> logger) : IProjectFacade
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("ProjectService");
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization) &&
            AuthenticationHeaderValue.TryParse(authorization, out var header))
        {
            client.DefaultRequestHeaders.Authorization = header;
        }

        return client;
    }

    public async Task<bool> ExistsByIdAsync(int projectId)
    {
        try
        {
            var project = await CreateClient().GetFromJsonAsync<ProjectInfo>(
                $"/api/v1/projects/{projectId}", JsonOptions);
            return project != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ProjectService ExistsByIdAsync for project {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<ProjectInfo?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            return await CreateClient().GetFromJsonAsync<ProjectInfo>(
                $"/api/v1/projects/{projectId}", JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ProjectService GetProjectByIdAsync for project {ProjectId}", projectId);
            return null;
        }
    }

    public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
    {
        try
        {
            var project = await CreateClient().GetFromJsonAsync<ProjectInfo>(
                $"/api/v1/projects/{projectId}", JsonOptions);
            return project != null && (project.ManagerId == userId || project.SupervisorId == userId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error calling ProjectService UserHasAccessToProjectAsync for user {UserId}, project {ProjectId}",
                userId,
                projectId);
            return false;
        }
    }

}
