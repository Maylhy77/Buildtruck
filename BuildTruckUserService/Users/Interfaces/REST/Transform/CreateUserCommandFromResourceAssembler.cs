using BuildTruckUserService.Users.Domain.Model.Commands;
using BuildTruckUserService.Users.Interfaces.REST.Resources;

namespace BuildTruckUserService.Users.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert CreateUserResource to CreateUserCommand
/// </summary>
/// <remarks>
/// Transforms REST DTO to Domain Command
/// </remarks>
public static class CreateUserCommandFromResourceAssembler
{
    /// <summary>
    /// Convert CreateUserResource to CreateUserCommand
    /// </summary>
    /// <param name="resource">The create user resource</param>
    /// <returns>The create user command</returns>
    public static CreateUserCommand ToCommandFromResource(CreateUserResource resource)
    {
        return new CreateUserCommand(
            resource.Name,
            resource.LastName,
            resource.Role,
            resource.PersonalEmail,
            resource.Phone
        );
    }
}