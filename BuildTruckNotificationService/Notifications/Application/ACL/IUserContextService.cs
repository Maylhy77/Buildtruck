namespace BuildTruckNotificationService.Notifications.Application.ACL;

public record UserInfo(int Id, string FullName, string Email, string Role, bool IsActive);

public interface IUserContextService
{
    Task<bool> UserExistsAsync(int userId);
    Task<string> GetUserNameAsync(int userId);
    Task<string> GetUserEmailAsync(int userId);
    Task<string> GetUserRoleAsync(int userId);
    Task<bool> IsUserActiveAsync(int userId);
    Task<IEnumerable<int>> GetAdminUsersAsync();
    Task<IEnumerable<int>> GetManagerUsersAsync();
    Task<IEnumerable<int>> GetUsersByRoleAsync(string role);
    Task<UserInfo?> GetUserByIdAsync(int userId);
}
