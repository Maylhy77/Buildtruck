namespace BuildTruckDocumentationService.Documentation.Application.Internal.OutboundServices;

/// <summary>
/// Service for external integrations and business logic for Documentation
/// </summary>
public class ExternalDocumentationService
{
    public static bool IsValidImageUrl(string imageUrl)
    {
        return !string.IsNullOrEmpty(imageUrl) && 
               imageUrl.Contains("cloudinary.com") &&
               Uri.TryCreate(imageUrl, UriKind.Absolute, out _);
    }

    public static bool IsValidTitle(string title)
    {
        return !string.IsNullOrEmpty(title) && 
               title.Length >= 3 && 
               title.Length <= 200 &&
               !title.Trim().Equals(string.Empty);
    }

    public static bool IsValidDescription(string description)
    {
        return !string.IsNullOrEmpty(description) && 
               description.Length >= 10 && 
               description.Length <= 1000 &&
               !description.Trim().Equals(string.Empty);
    }

    public static bool IsValidDate(DateTime date)
    {
        return date >= DateTime.Now.Date.AddYears(-5) && 
               date <= DateTime.Now.Date.AddDays(1);
    }

    public static Dictionary<string, object> GetProjectStatistics(
        IEnumerable<Domain.Model.Aggregates.Documentation> documentation)
    {
        var docs = documentation.ToList();
        
        var stats = new Dictionary<string, object>
        {
            ["totalDocuments"] = docs.Count,
            ["documentsWithImages"] = docs.Count(d => d.HasValidImage()),
            ["recentDocuments"] = docs.Count(d => 
                d.Date >= DateTime.Now.Date.AddDays(-7)),
            ["oldestDocument"] = docs.Any() 
                ? docs.Min(d => d.Date).ToString("yyyy-MM-dd")
                : string.Empty,
            ["newestDocument"] = docs.Any() 
                ? docs.Max(d => d.Date).ToString("yyyy-MM-dd")
                : string.Empty,
            ["documentsThisMonth"] = docs.Count(d => 
                d.Date.Year == DateTime.Now.Year && 
                d.Date.Month == DateTime.Now.Month)
        };

        return stats;
    }

    public static string GenerateUniqueFileName(string originalFileName, int documentationId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var extension = Path.GetExtension(originalFileName);
        return $"doc_{documentationId}_{timestamp}{extension}";
    }

    public static bool IsImageFile(string fileName)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    public static long GetMaxImageSizeBytes()
    {
        return 5 * 1024 * 1024; // 5MB
    }
}
