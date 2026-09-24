using System.ComponentModel.DataAnnotations;

namespace BuildTruckUserService.Auth.Interfaces.REST.Resources;

/// <summary>
/// Forgot password request resource
/// </summary>
/// <remarks>
/// Resource for requesting password reset email
/// </remarks>
public record ForgotPasswordRequest
{
    /// <summary>
    /// Corporate email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;
}