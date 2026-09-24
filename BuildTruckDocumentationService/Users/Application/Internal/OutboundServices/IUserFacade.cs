namespace BuildTruckDocumentationService.Users.Application.Internal.OutboundServices;

public interface IUserFacade
{
    Task<UserDto?> FindByIdAsync(int userId);
    Task<UserDto?> FindByEmailAsync(string email);
}
