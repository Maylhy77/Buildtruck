namespace BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

/// <summary>
/// Project Coordinates Value Object
/// </summary>
/// <remarks>
/// Represents geographical coordinates (latitude, longitude) for project location
/// </remarks>
public record ProjectCoordinates
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public ProjectCoordinates()
    {
        Latitude = 0.0;
        Longitude = 0.0;
    }

    public ProjectCoordinates(double latitude, double longitude)
    {
        if (!IsValidLatitude(latitude))
            throw new ArgumentException($"Invalid latitude: {latitude}. Must be between -90 and 90.", nameof(latitude));

        if (!IsValidLongitude(longitude))
            throw new ArgumentException($"Invalid longitude: {longitude}. Must be between -180 and 180.", nameof(longitude));

        Latitude = Math.Round(latitude, 8); // Precision to ~1mm
        Longitude = Math.Round(longitude, 8);
    }

    // Factory methods
    public static ProjectCoordinates? FromString(string? coordinatesString)
    {
        if (string.IsNullOrWhiteSpace(coordinatesString))
            return null;

        try
        {
            // Expected format: "lat,lng" or "lat, lng"
            var parts = coordinatesString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            if (double.TryParse(parts[0].Trim(), out var lat) && 
                double.TryParse(parts[1].Trim(), out var lng))
            {
                return new ProjectCoordinates(lat, lng);
            }
        }
        catch
        {
            // Invalid format, return null
        }

        return null;
    }

    public static ProjectCoordinates? FromObject(object? coordinatesObj)
    {
        if (coordinatesObj == null)
            return null;

        try
        {
            // Handle anonymous objects from frontend
            var properties = coordinatesObj.GetType().GetProperties();
            var latProp = properties.FirstOrDefault(p => p.Name.Equals("lat", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Name.Equals("latitude", StringComparison.OrdinalIgnoreCase));
            var lngProp = properties.FirstOrDefault(p => p.Name.Equals("lng", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Name.Equals("longitude", StringComparison.OrdinalIgnoreCase));

            if (latProp != null && lngProp != null)
            {
                var latValue = latProp.GetValue(coordinatesObj);
                var lngValue = lngProp.GetValue(coordinatesObj);

                if (latValue != null && lngValue != null &&
                    double.TryParse(latValue.ToString(), out var lat) &&
                    double.TryParse(lngValue.ToString(), out var lng))
                {
                    return new ProjectCoordinates(lat, lng);
                }
            }
        }
        catch
        {
            // Invalid object structure, return null
        }

        return null;
    }

    // Validation methods
    private static bool IsValidLatitude(double latitude) => latitude >= -90.0 && latitude <= 90.0;
    private static bool IsValidLongitude(double longitude) => longitude >= -180.0 && longitude <= 180.0;

    public bool IsValid() => IsValidLatitude(Latitude) && IsValidLongitude(Longitude);

    public bool IsZero() => Math.Abs(Latitude) < 0.0000001 && Math.Abs(Longitude) < 0.0000001;

    // Business logic methods
    public bool IsInPeru()
    {
        // Approximate Peru boundaries
        // Peru: Latitude -18.35 to -0.05, Longitude -81.33 to -68.65
        return Latitude >= -18.35 && Latitude <= -0.05 &&
               Longitude >= -81.33 && Longitude <= -68.65;
    }

    public double DistanceToLima()
    {
        // Lima coordinates: -12.0464, -77.0428
        const double limaLat = -12.0464;
        const double limaLng = -77.0428;
        
        return CalculateDistance(Latitude, Longitude, limaLat, limaLng);
    }

    // Calculate distance between two coordinates using Haversine formula
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371; // Earth radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    // String representations
    public string ToDecimalString() => $"{Latitude}, {Longitude}";
    
    public string ToGoogleMapsUrl() => $"https://www.google.com/maps?q={Latitude},{Longitude}";

    public override string ToString() => ToDecimalString();
}