using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record MaterialDto(int Id, string Name, decimal CurrentStock, decimal MinimumStock, int ProjectId);

public class MaterialContextService : IMaterialContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MaterialContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<int>> GetLowStockMaterialsAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendService");
            var response = await client.GetAsync($"/api/v1/materials?projectId={projectId}&lowStock=true");
            if (!response.IsSuccessStatusCode) return [];
            var materials = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialDto>>(JsonOptions);
            return materials?.Select(m => m.Id) ?? [];
        }
        catch { return []; }
    }

    public async Task<string> GetMaterialNameAsync(int materialId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendService");
            var response = await client.GetAsync($"/api/v1/materials/{materialId}");
            if (!response.IsSuccessStatusCode) return string.Empty;
            var material = await response.Content.ReadFromJsonAsync<MaterialDto>(JsonOptions);
            return material?.Name ?? string.Empty;
        }
        catch { return string.Empty; }
    }

    public async Task<decimal> GetMaterialStockAsync(int materialId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendService");
            var response = await client.GetAsync($"/api/v1/materials/{materialId}");
            if (!response.IsSuccessStatusCode) return 0;
            var material = await response.Content.ReadFromJsonAsync<MaterialDto>(JsonOptions);
            return material?.CurrentStock ?? 0;
        }
        catch { return 0; }
    }

    public async Task<decimal> GetMaterialMinimumStockAsync(int materialId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendService");
            var response = await client.GetAsync($"/api/v1/materials/{materialId}");
            if (!response.IsSuccessStatusCode) return 0;
            var material = await response.Content.ReadFromJsonAsync<MaterialDto>(JsonOptions);
            return material?.MinimumStock ?? 0;
        }
        catch { return 0; }
    }
}
