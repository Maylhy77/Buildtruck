namespace BuildTruckUserService.Auth.Domain.Model.Queries;

/// <summary>
/// Query for getting current authenticated user information
/// </summary>
/// <remarks>
/// Represents a request to get current user details from JWT token
/// </remarks>
public record GetCurrentUserQuery
{
    public int UserId { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public GetCurrentUserQuery(int userId, string? ipAddress = null, string? userAgent = null)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than zero.", nameof(userId));

        UserId = userId;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    // Métodos de dominio
    public bool IsValid() => UserId > 0;
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    // Para logging y auditoría
    public string GetAuditString()
    {
        var parts = new List<string> { $"UserId: {UserId}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}