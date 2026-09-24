using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;

namespace BuildTruckConfigurationService.Configurations.Domain.Repositories;

/// <summary>
/// Repository interface for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public interface IConfigurationSettingsRepository
{
    Task<ConfigurationSettings?> FindByIdAsync(int id);
    Task<ConfigurationSettings?> FindByUserIdAsync(int userId);
    Task AddAsync(ConfigurationSettings configurationSettings);
    Task UpdateAsync(ConfigurationSettings configurationSettings);
}
