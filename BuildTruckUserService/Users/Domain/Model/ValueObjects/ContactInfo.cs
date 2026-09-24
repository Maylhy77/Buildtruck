using System.Text.RegularExpressions;

namespace BuildTruckUserService.Users.Domain.Model.ValueObjects;

/// <summary>
/// ContactInfo Value Object
/// </summary>
/// <remarks>
/// Represents contact information including personal email and phone with validation for Peru
/// </remarks>
public record ContactInfo
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    // ✅ Regex flexible para teléfonos peruanos
    private static readonly Regex PhoneRegex = new(
        @"^(\+?51\s?)?[0-9]{7,9}$", // Acepta formatos peruanos
        RegexOptions.Compiled);

    public string? PersonalEmailAddress { get; init; }
    public string? Phone { get; init; }

    public ContactInfo()
    {
        PersonalEmailAddress = null;
        Phone = null;
    }

    public ContactInfo(string? personalEmail, string? phone)
    {
        // ✅ Validar email personal si se proporciona
        if (!string.IsNullOrWhiteSpace(personalEmail))
        {
            if (!EmailRegex.IsMatch(personalEmail))
                throw new ArgumentException("Invalid personal email format", nameof(personalEmail));
            PersonalEmailAddress = personalEmail.ToLower().Trim();
        }
        else
        {
            PersonalEmailAddress = null;
        }

        // ✅ Validar teléfono peruano si se proporciona
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var cleanPhone = NormalizePhone(phone);
            if (!PhoneRegex.IsMatch(cleanPhone))
                throw new ArgumentException("Invalid phone format. Examples: 999888777, +51999888777, 014445555", nameof(phone));
            Phone = cleanPhone;
        }
        else
        {
            Phone = null;
        }
    }

    /// <summary>
    /// Normalize phone number for Peru
    /// </summary>
    private static string NormalizePhone(string phone)
    {
        return phone
            .Replace(" ", "")     // Quitar espacios
            .Replace("-", "")     // Quitar guiones
            .Replace("(", "")     // Quitar paréntesis
            .Replace(")", "")
            .Trim();
    }

    public bool HasPersonalEmail => !string.IsNullOrWhiteSpace(PersonalEmailAddress);
    public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
}