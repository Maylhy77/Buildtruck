using System.ComponentModel.DataAnnotations;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;

public record UpdatePreferenceResource(
    [Required] string Context,
    [Required] bool InAppEnabled,
    [Required] bool EmailEnabled,
    [Required] string MinimumPriority
);
