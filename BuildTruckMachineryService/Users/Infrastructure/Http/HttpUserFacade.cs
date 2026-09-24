using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckMachineryService.Users.Application.Internal.OutboundServices;

namespace BuildTruckMachineryService.Users.Infrastructure.Http;

public class HttpUserFacade(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpUserFacade> logger) : IUserFacade
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient Client => httpClientFactory.CreateClient("UserService");

    public async Task<UserDto?> FindByIdAsync(int userId)
    {
        try
        {
            return await Client.GetFromJsonAsync<UserDto>($"/api/v1/users/{userId}", JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling UserService FindByIdAsync for user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> FindByEmailAsync(string email)
    {
        try
        {
            return await Client.GetFromJsonAsync<UserDto>(
                $"/api/v1/users?email={Uri.EscapeDataString(email)}", JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling UserService FindByEmailAsync for {Email}", email);
            return null;
        }
    }
}
