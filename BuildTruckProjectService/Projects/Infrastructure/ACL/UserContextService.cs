using BuildTruckProjectService.Projects.Application.ACL.Services;
using BuildTruckProjectService.Users.Application.Internal.OutboundServices;
using ProjectsUserDto = BuildTruckProjectService.Projects.Application.ACL.Services.UserDto;
using SharedUserDto = BuildTruckProjectService.Users.Application.Internal.OutboundServices.UserDto;

namespace BuildTruckProjectService.Projects.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(IUserFacade userFacade, ILogger<UserContextService> logger)
    {
        _userFacade = userFacade;
        _logger = logger;
    }

    public async Task<ProjectsUserDto?> FindByIdAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            if (user == null) return null;

            return new ProjectsUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                ProfileImageUrl = user.ProfileImageUrl,
                LastLogin = user.LastLogin.HasValue ? new DateTimeOffset(user.LastLogin.Value) : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> IsActiveUserAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.IsActive ?? false;
    }

    public async Task<bool> IsManagerAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.Role == "Manager";
    }

    public async Task<bool> IsSupervisorAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.Role == "Supervisor";
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.Role == "Admin";
    }

    public async Task<bool> IsSupervisorAvailableAsync(int supervisorId)
    {
        var user = await _userFacade.FindByIdAsync(supervisorId);
        return user != null && user.Role == "Supervisor" && user.IsActive;
    }

    public Task<int?> FindAvailableSupervisorAsync()
    {
        _logger.LogWarning("FindAvailableSupervisorAsync not fully implemented");
        return Task.FromResult<int?>(null);
    }

    public async Task<bool> AssignSupervisorToProjectAsync(int supervisorId, int projectId)
    {
        var user = await _userFacade.FindByIdAsync(supervisorId);
        return user != null && user.Role == "Supervisor" && user.IsActive;
    }

    public async Task<bool> ReleaseSupervisorFromProjectAsync(int supervisorId)
    {
        var user = await _userFacade.FindByIdAsync(supervisorId);
        return user != null && user.Role == "Supervisor";
    }

    public async Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            return await _userFacade.GetUserProfileImageUrlAsync(userId, size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile image for user {UserId}", userId);
            return string.Empty;
        }
    }
}
