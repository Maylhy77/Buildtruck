namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public class NotificationContent
{
    public string Title { get; private set; }
    public string Message { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? ActionText { get; private set; }
    public string? IconClass { get; private set; }

    private NotificationContent() { Title = "System"; Message = "Default"; }

    public NotificationContent(string title, string message, string? actionUrl = null,
        string? actionText = null, string? iconClass = null)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Notification" : title;
        Message = string.IsNullOrWhiteSpace(message) ? "No message" : message;
        ActionUrl = actionUrl;
        ActionText = actionText;
        IconClass = iconClass;
    }

    public bool HasAction() => !string.IsNullOrWhiteSpace(ActionUrl);
    public bool HasIcon() => !string.IsNullOrWhiteSpace(IconClass);
    public string GetDisplayActionText() => !string.IsNullOrWhiteSpace(ActionText) ? ActionText : "Ver detalles";
    public bool IsValidUrl()
    {
        if (string.IsNullOrWhiteSpace(ActionUrl)) return true;
        return Uri.TryCreate(ActionUrl, UriKind.RelativeOrAbsolute, out _);
    }
}
