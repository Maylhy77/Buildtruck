using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.Commands;

namespace BuildTruckUserService.Users.Domain.Services;

/// <summary>
/// User command service interface
/// </summary>
public interface IUserCommandService
{
    /// <summary>
    /// Handle create user command
    /// </summary>
    /// <param name="command">Create user command</param>
    /// <returns>Created user</returns>
    Task<User> Handle(CreateUserCommand command);
    
    /// <summary>
    /// Handle change password command
    /// </summary>
    /// <param name="command">Change password command</param>
    /// <returns>Updated user</returns>
    Task<User> Handle(ChangePasswordCommand command);
    
    /// <summary>
    /// Handle delete user command (physical deletion)
    /// </summary>
    /// <param name="command">Delete user command</param>
    /// <returns>Task</returns>
    Task Handle(DeleteUserCommand command);
    
    /// <summary>
    /// Handle upload profile image command (creates new or replaces existing)
    /// </summary>
    /// <param name="command">Upload profile image command</param>
    /// <returns>Updated user with new profile image URL</returns>
    Task<User> Handle(UploadProfileImageCommand command);
    
    /// <summary>
    /// Handle delete profile image command
    /// </summary>
    /// <param name="command">Delete profile image command</param>
    /// <returns>Updated user without profile image</returns>
    Task<User> Handle(DeleteProfileImageCommand command);
    
    /// <summary>
    /// Handle update user command
    /// </summary>
    /// <param name="command">Update user command</param>
    /// <returns>Updated user</returns>
    Task<User> Handle(UpdateUserCommand command);
}