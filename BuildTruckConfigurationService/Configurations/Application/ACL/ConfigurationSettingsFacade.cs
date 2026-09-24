using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.Commands;
using BuildTruckConfigurationService.Configurations.Domain.Services;
using BuildTruckConfigurationService.Configurations.Interfaces.ACL;

namespace BuildTruckConfigurationService.Configurations.Application.ACL;

/// <summary>
/// Facade for ConfigurationSettings operations
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsFacade : IConfigurationSettingsFacade
{
    private readonly IConfigurationSettingsCommandService _commandService;
    private readonly IConfigurationSettingsQueryService _queryService;

    public ConfigurationSettingsFacade(IConfigurationSettingsCommandService commandService, IConfigurationSettingsQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    public async Task<ConfigurationSettings> CreateConfigurationSettingsAsync(CreateConfigurationSettingsCommand command)
    {
        return await _commandService.Handle(command);
    }

    public async Task<ConfigurationSettings> UpdateConfigurationSettingsAsync(UpdateConfigurationSettingsCommand command)
    {
        return await _commandService.Handle(command);
    }

    public async Task<ConfigurationSettings?> GetConfigurationSettingsByUserIdAsync(int userId)
    {
        return await _queryService.GetByUserIdAsync(userId);
    }
}
