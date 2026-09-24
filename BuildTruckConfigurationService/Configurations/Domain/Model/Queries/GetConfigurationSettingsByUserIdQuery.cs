namespace BuildTruckConfigurationService.Configurations.Domain.Model.Queries;

/// <summary>
/// Query to retrieve configuration settings by user ID
/// </summary>
/// <remarks>Authors: Your Name Here</remarks>
public record GetConfigurationSettingsByUserIdQuery
{
    public string UserId { get; init; }

    public GetConfigurationSettingsByUserIdQuery(string userId)
    {
        UserId = userId;
    }
}
