using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.Commands;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;
using BuildTruckConfigurationService.Configurations.Domain.Repositories;
using BuildTruckConfigurationService.Configurations.Domain.Services;
using BuildTruckConfigurationService.Configurations.Application.ACL.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckConfigurationService.Configurations.Application.Internal.CommandServices;

/// <summary>
/// Command service for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsCommandService : IConfigurationSettingsCommandService
{
    private readonly IConfigurationSettingsRepository _configurationSettingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContextService _notificationContextService;

    public ConfigurationSettingsCommandService(
        IConfigurationSettingsRepository configurationSettingsRepository,
        IUnitOfWork unitOfWork,
        INotificationContextService notificationContextService)
    {
        _configurationSettingsRepository = configurationSettingsRepository;
        _unitOfWork = unitOfWork;
        _notificationContextService = notificationContextService;
    }

    public async Task<ConfigurationSettings> Handle(CreateConfigurationSettingsCommand command)
    {
        if (string.IsNullOrEmpty(command.UserIds.ToString()))
            throw new ArgumentException("UserId cannot be empty.");
        if (!Enum.IsDefined(typeof(Theme), command.Themes))
            throw new ArgumentException($"Invalid theme value: {command.Themes}. Must be a valid Theme.");
        if (!Enum.IsDefined(typeof(Plan), command.Plans))
            throw new ArgumentException($"Invalid plan value: {command.Plans}. Must be a valid Plan.");

        var existingConfig = await _configurationSettingsRepository.FindByUserIdAsync(command.UserIds);
        if (existingConfig != null)
            throw new ArgumentException("A configuration for this UserId already exists.");

        var configurationSettings = new ConfigurationSettings(
            command.UserIds,
            command.Themes,
            command.Plans,
            command.NotificationsEnables,
            command.EmailNotification,
            command.TutorialsCompleted ?? new TutorialProgress()
        );
        
        /*{
            UserId = command.UserIds,
            Themes = command.Themes,
            Plans = command.Plans,
            NotificationsEnable = command.NotificationsEnables,
            EmailNotifications = command.EmailNotification
        };
        */

        await _configurationSettingsRepository.AddAsync(configurationSettings);
        await _unitOfWork.CompleteAsync();

        try
        {
            await _notificationContextService.NotifyConfigurationCreatedAsync(
                configurationSettings.UserId);
        }
        catch
        {
            // Configuration persistence must not fail because notification delivery is unavailable.
        }

        return configurationSettings;
    }

    public async Task<ConfigurationSettings> Handle(UpdateConfigurationSettingsCommand command)
    {
        var configurationSettings = await _configurationSettingsRepository.FindByIdAsync(command.Ids)
                                    ?? throw new ArgumentException($"Configuration with Id {command.Ids} not found.");
        
        if (string.IsNullOrEmpty(command.UserIds.ToString()))
            throw new ArgumentException("UserId cannot be empty.");
        if (!Enum.IsDefined(typeof(Theme), command.Themes))
            throw new ArgumentException($"Invalid theme value: {command.Themes}. Must be a valid Theme.");
        if (!Enum.IsDefined(typeof(Plan), command.Plans))
            throw new ArgumentException($"Invalid plan value: {command.Plans}. Must be a valid Plan.");
        
        if (configurationSettings.UserId != command.UserIds)
            throw new ArgumentException("UserId cannot be changed.");

        configurationSettings.Themes = command.Themes;
        configurationSettings.Plans = command.Plans;
        configurationSettings.NotificationsEnable = command.NotificationsEnables;
        configurationSettings.EmailNotifications = command.EmailNotification;
        configurationSettings.TutorialsCompleted = command.TutorialsCompleted;
        configurationSettings.UpdatedAt = DateTime.UtcNow;

        await _configurationSettingsRepository.UpdateAsync(configurationSettings);
        await _unitOfWork.CompleteAsync();
        
        
        return configurationSettings;
    }
}
