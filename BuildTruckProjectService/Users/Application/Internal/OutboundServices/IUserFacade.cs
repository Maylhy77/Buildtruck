namespace BuildTruckProjectService.Users.Application.Internal.OutboundServices;

public interface IUserFacade
{
    Task<UserDto?> FindByIdAsync(int userId);
    Task<UserDto?> FindByEmailAsync(string email);
    Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200);
}
