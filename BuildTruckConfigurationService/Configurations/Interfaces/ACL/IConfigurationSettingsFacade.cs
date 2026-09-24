using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.Commands;

namespace BuildTruckConfigurationService.Configurations.Interfaces.ACL;

/// <summary>
/// Interface for ConfigurationSettings facade
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public interface IConfigurationSettingsFacade
{
    Task<ConfigurationSettings> CreateConfigurationSettingsAsync(CreateConfigurationSettingsCommand command);
    Task<ConfigurationSettings> UpdateConfigurationSettingsAsync(UpdateConfigurationSettingsCommand command);
    Task<ConfigurationSettings?> GetConfigurationSettingsByUserIdAsync(int userId);
}
