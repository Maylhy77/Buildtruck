namespace BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string ProfileImagesFolder { get; set; } = "buildtruck/profiles/";
    public string MachineryImagesFolder { get; set; } = "buildtruck/machinery/";
    public string IncidentImagesFolder { get; set; } = "buildtruck/incidents/";
    public int MaxFileSizeBytes { get; set; } = 5242880;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png"];
    public string TransformationPreset { get; set; } = "profile_pic";

    public bool IsValid =>
        !string.IsNullOrEmpty(CloudName) &&
        !string.IsNullOrEmpty(ApiKey) &&
        !string.IsNullOrEmpty(ApiSecret);
}
