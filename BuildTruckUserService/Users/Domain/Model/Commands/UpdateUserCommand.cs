namespace BuildTruckUserService.Users.Domain.Model.Commands;

/// <summary>
/// Command for updating user basic information
/// Corporate email is auto-generated and cannot be changed directly
/// </summary>
/// <param name="UserId">The ID of the user to update</param>
/// <param name="Name">New first name (optional - null means no change)</param>
/// <param name="LastName">New last name (optional - null means no change)</param>
/// <param name="PersonalEmail">New personal email (optional - null means no change)</param>
/// <param name="Role">New role (optional - null means no change)</param>
public record UpdateUserCommand(
    int UserId,
    string? Name,
    string? LastName,
    string? PersonalEmail,
    string? Role)
{
    /// <summary>
    /// Indicates if name will be updated (affects corporate email generation)
    /// </summary>
    public bool WillUpdateName => !string.IsNullOrWhiteSpace(Name) || !string.IsNullOrWhiteSpace(LastName);
    
    /// <summary>
    /// Indicates if any field will be updated
    /// </summary>
    public bool HasChanges => !string.IsNullOrWhiteSpace(Name) || 
                              !string.IsNullOrWhiteSpace(LastName) || 
                              !string.IsNullOrWhiteSpace(PersonalEmail) || 
                              !string.IsNullOrWhiteSpace(Role);
}