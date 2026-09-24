using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;

namespace BuildTruckConfigurationService.Configurations.Domain.Services;

/// <summary>
/// Query service interface for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public interface IConfigurationSettingsQueryService
{
    Task<ConfigurationSettings?> GetByUserIdAsync(int userId);
}
