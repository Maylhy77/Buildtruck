using System.ComponentModel.DataAnnotations;

namespace BuildTruckUserService.Users.Interfaces.REST.Resources;

/// <summary>
/// Request for updating user information
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// New first name (optional)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New last name (optional)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// New personal email (optional)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? PersonalEmail { get; set; }

    /// <summary>
    /// New role (optional)
    /// </summary>
    public string? Role { get; set; }
}