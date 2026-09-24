using BuildTruckUserService.Auth.Domain.Model.ValueObjects;

namespace BuildTruckUserService.Auth.Domain.Model.Commands;

/// <summary>
/// Command for user authentication
/// </summary>
/// <remarks>
/// Represents a sign-in request with email and password
/// </remarks>
public record SignInCommand
{
    public LoginCredentials Credentials { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public SignInCommand(string email, string password, string? ipAddress = null, string? userAgent = null)
    {
        Credentials = new LoginCredentials(email, password);
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public SignInCommand(LoginCredentials credentials, string? ipAddress = null, string? userAgent = null)
    {
        Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    // Propiedades de conveniencia
    public string Email => Credentials.Email;
    public string Password => Credentials.Password;
    
    // Métodos de dominio
    public bool IsValid() => Credentials.IsValid();
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    // Para logging y auditoría (sin exponer la password)
    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}