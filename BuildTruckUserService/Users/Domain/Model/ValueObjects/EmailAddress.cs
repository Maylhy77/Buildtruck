using System.Text.RegularExpressions;

namespace BuildTruckUserService.Users.Domain.Model.ValueObjects;

/// <summary>
/// EmailAddress Value Object
/// </summary>
/// <remarks>
/// Represents an email address with validation and corporate email generation
/// </remarks>
public record EmailAddress
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public string Address { get; init; }

    public EmailAddress()
    {
        Address = string.Empty;
    }

    public EmailAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email address cannot be empty", nameof(address));

        if (!EmailRegex.IsMatch(address))
            throw new ArgumentException("Invalid email format", nameof(address));

        Address = address.ToLower().Trim();
    }

    /// <summary>
    /// Generate corporate email for BuildTruck
    /// </summary>
    public static EmailAddress GenerateCorporateEmail(PersonName personName)
    {
        var normalizedFirstName = NormalizeName(personName.FirstName);
        var normalizedLastName = NormalizeName(personName.LastName);
        
        var corporateEmail = $"{normalizedFirstName}.{normalizedLastName}@buildtruck.com";
        return new EmailAddress(corporateEmail);
    }

    /// <summary>
    /// Normalize name for email generation (removes accents, special chars)
    /// </summary>
    private static string NormalizeName(string name)
    {
        return name
            .ToLower()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Aggregate("", (current, c) => current + c)
            .Replace(" ", "")
            .Where(char.IsLetterOrDigit)
            .Aggregate("", (current, c) => current + c);
    }

    public override string ToString() => Address;
}