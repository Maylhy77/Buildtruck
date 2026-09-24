using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record PersonnelAttendanceDto(int ProjectId, decimal AttendanceRate);
file record PersonnelDto(int Id, string FirstName, string LastName);

public class PersonnelContextService : IPersonnelContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PersonnelContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<decimal> GetAttendanceRateAsync(int projectId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PersonnelService");
            var response = await client.GetAsync($"/api/v1/personnel/attendance-rate?projectId={projectId}");
            if (!response.IsSuccessStatusCode) return 1.0m;
            var dto = await response.Content.ReadFromJsonAsync<PersonnelAttendanceDto>(JsonOptions);
            return dto?.AttendanceRate ?? 1.0m;
        }
        catch { return 1.0m; }
    }

    public async Task<string?> GetPersonnelNameAsync(int personnelId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PersonnelService");
            var response = await client.GetAsync($"/api/v1/personnel/{personnelId}");
            if (!response.IsSuccessStatusCode) return null;
            var dto = await response.Content.ReadFromJsonAsync<PersonnelDto>(JsonOptions);
            return dto == null ? null : $"{dto.FirstName} {dto.LastName}".Trim();
        }
        catch { return null; }
    }
}
