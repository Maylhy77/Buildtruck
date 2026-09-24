using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record IncidentDto(int Id, string Title, string Status, string Severity, int ProjectId);

public class IncidentContextService : IIncidentContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IncidentContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int> GetOpenIncidentsCountAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IncidentService");
            var response = await client.GetAsync($"/api/v1/incidents?projectId={projectId}&status=open");
            if (!response.IsSuccessStatusCode) return 0;
            var incidents = await response.Content.ReadFromJsonAsync<IEnumerable<IncidentDto>>(JsonOptions);
            return incidents?.Count() ?? 0;
        }
        catch { return 0; }
    }

    public async Task<string?> GetIncidentTitleAsync(int incidentId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IncidentService");
            var response = await client.GetAsync($"/api/v1/incidents/{incidentId}");
            if (!response.IsSuccessStatusCode) return null;
            var dto = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
            return dto?.Title;
        }
        catch { return null; }
    }
}
