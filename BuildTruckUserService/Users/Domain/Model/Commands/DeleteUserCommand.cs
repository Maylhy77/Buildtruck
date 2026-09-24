namespace BuildTruckUserService.Users.Domain.Model.Commands;

/// <summary>
/// Command for deleting a user (physical deletion)
/// </summary>
/// <param name="UserId">The ID of the user to delete</param>
public record DeleteUserCommand(int UserId);