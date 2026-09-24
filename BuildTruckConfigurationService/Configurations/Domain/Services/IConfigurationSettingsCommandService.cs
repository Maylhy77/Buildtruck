using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.Commands;

namespace BuildTruckConfigurationService.Configurations.Domain.Services;

/// <summary>
/// Command service interface for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public interface IConfigurationSettingsCommandService
{
    Task<ConfigurationSettings> Handle(CreateConfigurationSettingsCommand command);
    Task<ConfigurationSettings> Handle(UpdateConfigurationSettingsCommand command);
}
