using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record MachineryDto(int Id, string Name, string Status, int? ProjectId);

public class MachineryContextService : IMachineryContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MachineryContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int> GetActiveMachineryCountAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MachineryService");
            var response = await client.GetAsync($"/api/v1/machinery?projectId={projectId}&status=active");
            if (!response.IsSuccessStatusCode) return 0;
            var machinery = await response.Content.ReadFromJsonAsync<IEnumerable<MachineryDto>>(JsonOptions);
            return machinery?.Count() ?? 0;
        }
        catch { return 0; }
    }

    public async Task<string?> GetMachineryNameAsync(int machineryId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MachineryService");
            var response = await client.GetAsync($"/api/v1/machinery/{machineryId}");
            if (!response.IsSuccessStatusCode) return null;
            var dto = await response.Content.ReadFromJsonAsync<MachineryDto>(JsonOptions);
            return dto?.Name;
        }
        catch { return null; }
    }
}
