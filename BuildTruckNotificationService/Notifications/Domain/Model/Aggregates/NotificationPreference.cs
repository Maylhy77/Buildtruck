using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;

public partial class NotificationPreference
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public NotificationContext Context { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public NotificationPriority MinimumPriority { get; private set; }

    public NotificationPreference()
    {
        Context = NotificationContext.System;
        InAppEnabled = true;
        EmailEnabled = false;
        MinimumPriority = NotificationPriority.Normal;
    }

    public NotificationPreference(int userId, NotificationContext context, bool inAppEnabled = true,
        bool emailEnabled = false, NotificationPriority? minimumPriority = null)
    {
        UserId = userId;
        Context = context ?? throw new ArgumentNullException(nameof(context));
        InAppEnabled = inAppEnabled;
        EmailEnabled = emailEnabled;
        MinimumPriority = minimumPriority ?? NotificationPriority.Normal;
    }

    public void UpdatePreferences(bool inAppEnabled, bool emailEnabled, NotificationPriority minimumPriority)
    {
        InAppEnabled = inAppEnabled;
        EmailEnabled = emailEnabled;
        MinimumPriority = minimumPriority ?? throw new ArgumentNullException(nameof(minimumPriority));
    }

    public bool ShouldReceiveInApp(NotificationPriority priority) =>
        InAppEnabled && priority.Level >= MinimumPriority.Level;

    public bool ShouldReceiveEmail(NotificationPriority priority) =>
        EmailEnabled && priority.Level >= MinimumPriority.Level;

    public bool ShouldReceiveNotification(NotificationPriority priority) =>
        ShouldReceiveInApp(priority) || ShouldReceiveEmail(priority);

    public void DisableAllNotifications() { InAppEnabled = false; EmailEnabled = false; }

    public void EnableDefaultSettings()
    {
        InAppEnabled = true;
        EmailEnabled = false;
        MinimumPriority = NotificationPriority.Normal;
    }

    public void EnableOnlyCritical()
    {
        InAppEnabled = true;
        EmailEnabled = true;
        MinimumPriority = NotificationPriority.Critical;
    }
}
