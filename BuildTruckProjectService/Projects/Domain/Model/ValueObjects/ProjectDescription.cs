namespace BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

/// <summary>
/// Project Description Value Object
/// </summary>
/// <remarks>
/// Represents and validates project descriptions with business rules
/// </remarks>
public record ProjectDescription
{
    public string Description { get; init; }

    public ProjectDescription()
    {
        Description = string.Empty;
    }

    public ProjectDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Project description cannot be null or empty.", nameof(description));

        if (description.Trim().Length < 10)
            throw new ArgumentException("Project description must be at least 10 characters long.", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Project description cannot exceed 1000 characters.", nameof(description));

        // Normalize description
        Description = NormalizeDescription(description);
    }

    private static string NormalizeDescription(string description)
    {
        // Trim and normalize line endings
        return description.Trim()
                         .Replace("\r\n", "\n")
                         .Replace("\r", "\n");
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Description) && 
               Description.Length >= 10 && 
               Description.Length <= 1000;
    }

    public string GetSummary(int maxLength = 100)
    {
        if (Description.Length <= maxLength)
            return Description;

        var truncated = Description.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        
        if (lastSpace > maxLength - 20) // If last space is near the end
            truncated = truncated.Substring(0, lastSpace);
        
        return truncated + "...";
    }

    public override string ToString() => Description;

    // Implicit conversion operators for convenience
    public static implicit operator string(ProjectDescription projectDescription) => projectDescription.Description;
    public static implicit operator ProjectDescription(string description) => new(description);
}