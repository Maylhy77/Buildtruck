using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record SettingDto(string Key, string Value);

public class ConfigurationContextService : IConfigurationContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ConfigurationContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ConfigurationService");
            var response = await client.GetAsync($"/api/v1/configurations/{Uri.EscapeDataString(key)}");
            if (!response.IsSuccessStatusCode) return null;
            var dto = await response.Content.ReadFromJsonAsync<SettingDto>(JsonOptions);
            return dto?.Value;
        }
        catch { return null; }
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureKey)
    {
        var value = await GetSettingAsync(featureKey);
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
