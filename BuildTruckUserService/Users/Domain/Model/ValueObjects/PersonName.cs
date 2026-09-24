namespace BuildTruckUserService.Users.Domain.Model.ValueObjects;

/// <summary>
/// PersonName Value Object
/// </summary>
/// <remarks>
/// Represents a person's name with validation and computed properties
/// </remarks>
public record PersonName
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();

    public PersonName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}