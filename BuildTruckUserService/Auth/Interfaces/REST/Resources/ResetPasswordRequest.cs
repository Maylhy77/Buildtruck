using System.ComponentModel.DataAnnotations;

namespace BuildTruckUserService.Auth.Interfaces.REST.Resources;

/// <summary>
/// Reset password request resource
/// </summary>
/// <remarks>
/// Resource for resetting password with token
/// </remarks>
public record ResetPasswordRequest
{
    /// <summary>
    /// Password reset token from email
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    [StringLength(1000, ErrorMessage = "Token cannot exceed 1000 characters")]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Corporate email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string NewPassword { get; init; } = string.Empty;
}