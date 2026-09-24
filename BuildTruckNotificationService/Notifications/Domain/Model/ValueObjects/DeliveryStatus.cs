namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public record DeliveryStatus
{
    public static readonly DeliveryStatus Pending = new("PENDING");
    public static readonly DeliveryStatus Sent = new("SENT");
    public static readonly DeliveryStatus Failed = new("FAILED");
    public static readonly DeliveryStatus Retrying = new("RETRYING");

    public string Value { get; init; }

    private DeliveryStatus(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
    private DeliveryStatus() { Value = string.Empty; }

    public bool IsPending() => Value == Pending.Value;
    public bool IsSent() => Value == Sent.Value;
    public bool IsFailed() => Value == Failed.Value;
    public bool IsRetrying() => Value == Retrying.Value;
    public bool IsComplete() => IsSent() || IsFailed();
    public bool CanRetry() => IsFailed() || IsRetrying();

    public static DeliveryStatus FromString(string value) =>
        GetAllStatuses().FirstOrDefault(s => s.Value == value)
        ?? throw new ArgumentException($"Invalid delivery status: {value}");

    public static IEnumerable<DeliveryStatus> GetAllStatuses() =>
        new[] { Pending, Sent, Failed, Retrying };
}
