namespace BuildTruckUserService.Users.Domain.Model.Commands;

/// <summary>
/// Command for deleting user profile image
/// </summary>
/// <param name="UserId">The ID of the user whose profile image will be deleted</param>
public record DeleteProfileImageCommand(int UserId);