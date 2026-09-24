using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Commands;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Application.Internal.CommandServices;

public class NotificationPreferenceCommandService : INotificationPreferenceCommandService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationPreferenceCommandService(INotificationPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePreferenceCommand command)
    {
        var preference = await _preferenceRepository.FindByUserIdAndContextAsync(command.UserId, command.Context);

        if (preference == null)
        {
            preference = new NotificationPreference(command.UserId, command.Context,
                command.InAppEnabled, command.EmailEnabled, command.MinimumPriority);
            await _preferenceRepository.AddAsync(preference);
        }
        else
        {
            preference.UpdatePreferences(command.InAppEnabled, command.EmailEnabled, command.MinimumPriority);
            _preferenceRepository.Update(preference);
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task CreateDefaultPreferencesAsync(int userId)
    {
        var contexts = NotificationContext.GetAllContexts();
        foreach (var context in contexts)
        {
            var exists = await _preferenceRepository.ExistsByUserIdAndContextAsync(userId, context);
            if (!exists)
            {
                var preference = new NotificationPreference(userId, context);
                await _preferenceRepository.AddAsync(preference);
            }
        }
        await _unitOfWork.CompleteAsync();
    }

    public async Task ResetToDefaultsAsync(int userId)
    {
        var preferences = await _preferenceRepository.FindByUserIdAsync(userId);
        foreach (var preference in preferences)
        {
            preference.EnableDefaultSettings();
            _preferenceRepository.Update(preference);
        }
        await _unitOfWork.CompleteAsync();
    }
}
