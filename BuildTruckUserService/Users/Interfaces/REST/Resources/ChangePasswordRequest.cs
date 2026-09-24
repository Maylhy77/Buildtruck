using System.ComponentModel.DataAnnotations;

namespace BuildTruckUserService.Users.Interfaces.REST.Resources;

/// <summary>
/// Change password request DTO
/// </summary>
public record ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}