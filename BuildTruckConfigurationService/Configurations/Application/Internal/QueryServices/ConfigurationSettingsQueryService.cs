using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Repositories;
using BuildTruckConfigurationService.Configurations.Domain.Services;

namespace BuildTruckConfigurationService.Configurations.Application.Internal.QueryServices;

/// <summary>
/// Query service for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsQueryService : IConfigurationSettingsQueryService
{
    private readonly IConfigurationSettingsRepository _configurationSettingsRepository;

    public ConfigurationSettingsQueryService(IConfigurationSettingsRepository configurationSettingsRepository)
    {
        _configurationSettingsRepository = configurationSettingsRepository;
    }

    public async Task<ConfigurationSettings?> GetByUserIdAsync(int userId)
    {
        return await _configurationSettingsRepository.FindByUserIdAsync(userId);
    }
}
