namespace BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Configuration;

/// <summary>
/// JWT Token configuration settings
/// </summary>
/// <remarks>
/// Configuration for JWT token generation and validation
/// </remarks>
public class TokenSettings
{
    public const string SectionName = "TokenSettings";

    /// <summary>
    /// Secret key for JWT signing (must be at least 256 bits / 32 characters)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer
    /// </summary>
    public string Issuer { get; set; } = "BuildTruckBack";

    /// <summary>
    /// JWT token audience
    /// </summary>
    public string Audience { get; set; } = "BuildTruckBack-Users";

    /// <summary>
    /// Token expiration time in hours (default: 8 hours for full workday)
    /// </summary>
    public int ExpirationHours { get; set; } = 8;

    /// <summary>
    /// Clock skew tolerance in minutes (default: 5 minutes)
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Whether to require token expiration
    /// </summary>
    public bool RequireExpirationTime { get; set; } = true;

    /// <summary>
    /// Whether to require signed tokens
    /// </summary>
    public bool RequireSignedTokens { get; set; } = true;

    // Validation methods
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) &&
               SecretKey.Length >= 32 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               ExpirationHours > 0 &&
               ClockSkewMinutes >= 0;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SecretKey))
            errors.Add("SecretKey is required");
        else if (SecretKey.Length < 32)
            errors.Add("SecretKey must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(Issuer))
            errors.Add("Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            errors.Add("Audience is required");

        if (ExpirationHours <= 0)
            errors.Add("ExpirationHours must be greater than 0");

        if (ClockSkewMinutes < 0)
            errors.Add("ClockSkewMinutes cannot be negative");

        return errors;
    }
}