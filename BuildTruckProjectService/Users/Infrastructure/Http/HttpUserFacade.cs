using System.Net.Http.Json;
using BuildTruckProjectService.Users.Application.Internal.OutboundServices;

namespace BuildTruckProjectService.Users.Infrastructure.Http;

public class HttpUserFacade(IHttpClientFactory httpClientFactory) : IUserFacade
{
    private HttpClient Client => httpClientFactory.CreateClient("UserService");

    public async Task<UserDto?> FindByIdAsync(int userId)
    {
        try
        {
            return await Client.GetFromJsonAsync<UserDto>($"/api/v1/users/{userId}");
        }
        catch { return null; }
    }

    public async Task<UserDto?> FindByEmailAsync(string email)
    {
        try
        {
            return await Client.GetFromJsonAsync<UserDto>($"/api/v1/users?email={Uri.EscapeDataString(email)}");
        }
        catch { return null; }
    }

    public async Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<ProfileImageResponse>(
                $"/api/v1/users/{userId}/profile-image-url?size={size}");
            return response?.Url ?? string.Empty;
        }
        catch { return string.Empty; }
    }

    private record ProfileImageResponse(string Url);
}
