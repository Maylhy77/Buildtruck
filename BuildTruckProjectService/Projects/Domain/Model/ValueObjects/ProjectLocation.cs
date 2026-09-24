namespace BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

/// <summary>
/// Project Location Value Object
/// </summary>
/// <remarks>
/// Represents project location with validation
/// </remarks>
public record ProjectLocation
{
    public string Location { get; init; }

    public ProjectLocation()
    {
        Location = string.Empty;
    }

    public ProjectLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Project location cannot be null or empty.", nameof(location));

        if (location.Trim().Length < 3)
            throw new ArgumentException("Project location must be at least 3 characters long.", nameof(location));

        if (location.Length > 200)
            throw new ArgumentException("Project location cannot exceed 200 characters.", nameof(location));

        Location = NormalizeLocation(location);
    }

    private static string NormalizeLocation(string location)
    {
        // Remove extra whitespaces and trim
        return string.Join(" ", location.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Location) && 
               Location.Length >= 3 && 
               Location.Length <= 200;
    }

    public bool IsInPeru()
    {
        var locationLower = Location.ToLowerInvariant();
        return locationLower.Contains("peru") || 
               locationLower.Contains("perú") || 
               locationLower.Contains("lima") ||
               locationLower.Contains("arequipa") ||
               locationLower.Contains("cusco") ||
               locationLower.Contains("trujillo");
    }

    public string GetShortLocation(int maxLength = 50)
    {
        if (Location.Length <= maxLength)
            return Location;

        var truncated = Location.Substring(0, maxLength);
        var lastComma = truncated.LastIndexOf(',');
        var lastSpace = truncated.LastIndexOf(' ');
        
        var cutPoint = Math.Max(lastComma, lastSpace);
        if (cutPoint > maxLength - 15) // If cut point is reasonable
            truncated = truncated.Substring(0, cutPoint);
        
        return truncated + "...";
    }

    public override string ToString() => Location;

    // Implicit conversion operators for convenience
    public static implicit operator string(ProjectLocation projectLocation) => projectLocation.Location;
    public static implicit operator ProjectLocation(string location) => new(location);
}