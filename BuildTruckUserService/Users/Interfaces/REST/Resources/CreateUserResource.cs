namespace BuildTruckUserService.Users.Interfaces.REST.Resources;

/// <summary>
/// Create User Resource DTO
/// </summary>
/// <remarks>
/// Represents the data needed to create a new user via REST API
/// </remarks>
public record CreateUserResource(
    string Name,
    string LastName,
    string Role,
    string? PersonalEmail = null,
    string? Phone = null
);