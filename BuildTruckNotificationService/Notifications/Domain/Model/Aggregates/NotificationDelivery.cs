using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;

public partial class NotificationDelivery
{
    public int Id { get; private set; }
    public int NotificationId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public NotificationDelivery() { Channel = NotificationChannel.InApp; Status = DeliveryStatus.Pending; AttemptCount = 0; }

    public NotificationDelivery(int notificationId, NotificationChannel channel)
    {
        NotificationId = notificationId;
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Status = DeliveryStatus.Pending;
        AttemptCount = 0;
    }

    public void MarkAsSent() { Status = DeliveryStatus.Sent; SentAt = DateTime.UtcNow; ErrorMessage = null; }

    public void MarkAsFailed(string errorMessage)
    {
        Status = DeliveryStatus.Failed;
        ErrorMessage = errorMessage;
        LastAttemptAt = DateTime.UtcNow;
        AttemptCount++;
    }

    public void MarkAsRetrying() { Status = DeliveryStatus.Retrying; LastAttemptAt = DateTime.UtcNow; AttemptCount++; }

    public bool CanRetry() => AttemptCount < 3 && Status != DeliveryStatus.Sent;
    public bool IsMaxAttemptsReached() => AttemptCount >= 3;
    public bool IsPending() => Status == DeliveryStatus.Pending || Status == DeliveryStatus.Retrying;
    public bool IsSuccessful() => Status == DeliveryStatus.Sent;
    public bool HasFailed() => Status == DeliveryStatus.Failed;

    public TimeSpan GetTimeSinceLastAttempt() =>
        LastAttemptAt.HasValue ? DateTime.UtcNow - LastAttemptAt.Value : TimeSpan.Zero;

    public bool ShouldRetryNow()
    {
        if (!CanRetry()) return false;
        var timeSinceLastAttempt = GetTimeSinceLastAttempt();
        var minimumWaitTime = TimeSpan.FromMinutes(Math.Pow(2, AttemptCount));
        return timeSinceLastAttempt >= minimumWaitTime;
    }
}
