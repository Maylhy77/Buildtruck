namespace BuildTruckNotificationService.Shared.Infrastructure.Tokens.JWT;

public class TokenSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 8;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public int ClockSkewMinutes { get; set; } = 5;

    public bool IsValid() => !string.IsNullOrWhiteSpace(SecretKey)
                             && !string.IsNullOrWhiteSpace(Issuer)
                             && !string.IsNullOrWhiteSpace(Audience);
}
