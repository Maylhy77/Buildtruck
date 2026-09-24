using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

file record UserDto(int Id, string FirstName, string LastName, string Email,
    string? PersonalEmail, string Role, bool IsActive)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class UserContextService : IUserContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UserContextService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<UserInfo?> GetUserByIdAsync(int userId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/v1/users/{userId}");
            if (!response.IsSuccessStatusCode) return null;
            var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
            return dto == null ? null : new UserInfo(dto.Id, dto.FullName, dto.PersonalEmail ?? dto.Email, dto.Role, dto.IsActive);
        }
        catch { return null; }
    }

    public async Task<bool> UserExistsAsync(int userId) =>
        (await GetUserByIdAsync(userId)) != null;

    public async Task<string> GetUserNameAsync(int userId) =>
        (await GetUserByIdAsync(userId))?.FullName ?? string.Empty;

    public async Task<string> GetUserEmailAsync(int userId) =>
        (await GetUserByIdAsync(userId))?.Email ?? string.Empty;

    public async Task<string> GetUserRoleAsync(int userId) =>
        (await GetUserByIdAsync(userId))?.Role ?? string.Empty;

    public async Task<bool> IsUserActiveAsync(int userId) =>
        (await GetUserByIdAsync(userId))?.IsActive ?? false;

    public async Task<IEnumerable<int>> GetAdminUsersAsync() =>
        await GetUsersByRoleAsync("Admin");

    public async Task<IEnumerable<int>> GetManagerUsersAsync() =>
        await GetUsersByRoleAsync("Manager");

    public async Task<IEnumerable<int>> GetUsersByRoleAsync(string role)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/v1/users?role={Uri.EscapeDataString(role)}&activeOnly=true");
            if (!response.IsSuccessStatusCode) return [];
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>(JsonOptions);
            return users?.Select(u => u.Id) ?? [];
        }
        catch { return []; }
    }
}
