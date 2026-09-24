namespace BuildTruckIncidentService.Shared.Infrastructure.Tokens.JWT;

public class TokenSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "BuildTruckBack";
    public string Audience { get; set; } = "BuildTruckBack-Users";
    public int ExpirationHours { get; set; } = 8;
    public int ClockSkewMinutes { get; set; } = 5;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(SecretKey) &&
        SecretKey.Length >= 32 &&
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(Audience) &&
        ExpirationHours > 0 &&
        ClockSkewMinutes >= 0;
}
