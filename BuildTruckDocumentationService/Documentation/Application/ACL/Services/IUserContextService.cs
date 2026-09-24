namespace BuildTruckDocumentationService.Documentation.Application.ACL.Services;

/// <summary>
/// Anti-Corruption Layer service to communicate with User context
/// </summary>
public interface IUserContextService
{
    Task<bool> UserExistsAsync(int userId);
    Task<string?> GetUserEmailAsync(int userId);
    Task<bool> UserHasPermissionAsync(int userId, string permission);
}