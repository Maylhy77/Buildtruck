namespace BuildTruckUserService.Users.Domain.Model.Commands;

/**
 * <summary>
 *     The create user command
 * </summary>
 * <remarks>
 *     This command is used to create a new user in the BuildTruck platform
 *     Only admins can execute this command
 * </remarks>
 */
public record CreateUserCommand(
    string Name,
    string LastName,
    string Role, // Admin, Manager, Supervisor, Worker
    string? PersonalEmail = null,
    string? Phone = null
);