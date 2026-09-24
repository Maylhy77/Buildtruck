namespace BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

/// <summary>
/// Project Name Value Object
/// </summary>
/// <remarks>
/// Represents and validates project names with business rules
/// </remarks>
public record ProjectName
{
    public string Name { get; init; }

    public ProjectName()
    {
        Name = string.Empty;
    }

    public ProjectName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be null or empty.", nameof(name));

        if (name.Trim().Length < 2)
            throw new ArgumentException("Project name must be at least 2 characters long.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Project name cannot exceed 100 characters.", nameof(name));

        // Remove extra spaces and normalize
        Name = NormalizeName(name);
    }

    private static string NormalizeName(string name)
    {
        // Remove extra whitespaces and trim
        return string.Join(" ", name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && 
               Name.Length >= 2 && 
               Name.Length <= 100;
    }

    public override string ToString() => Name;

    // Implicit conversion operators for convenience
    public static implicit operator string(ProjectName projectName) => projectName.Name;
    public static implicit operator ProjectName(string name) => new(name);
}