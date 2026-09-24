namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("UserService");

    public UserContextService(IHttpClientFactory httpClientFactory, ILogger<UserContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsValidManagerAsync(int userId)
    {
        try
        {
            var user = await Client.GetFromJsonAsync<UserDto>($"/api/v1/users/{userId}");
            return user != null && user.Role.Equals("manager", StringComparison.OrdinalIgnoreCase) && user.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating manager {UserId}", userId);
            return false;
        }
    }

    public async Task<Dictionary<string, object>?> GetManagerInfoAsync(int managerId)
    {
        try
        {
            var user = await Client.GetFromJsonAsync<UserDto>($"/api/v1/users/{managerId}");
            if (user == null) return null;

            return new Dictionary<string, object>
            {
                ["Id"] = user.Id,
                ["Name"] = user.FirstName ?? string.Empty,
                ["LastName"] = user.LastName ?? string.Empty,
                ["FullName"] = $"{user.FirstName} {user.LastName}".Trim(),
                ["Email"] = user.Email ?? string.Empty,
                ["Role"] = user.Role ?? string.Empty,
                ["IsActive"] = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting manager info for {ManagerId}", managerId);
            return null;
        }
    }

    private record UserDto(int Id, string? FirstName, string? LastName, string? Email, string? Role, bool IsActive);
}
