namespace BuildTruckUserService.Auth.Interfaces.REST.Resources;

/// <summary>
/// Password reset response resource
/// </summary>
/// <remarks>
/// Resource for password reset operation responses
/// </remarks>
public record PasswordResetResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if operation was successful
    /// </summary>
    public bool Success { get; init; }
}