using BuildTruckNotificationService.Notifications.Application.ACL.Services;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Messaging;

/// <summary>
/// Se usa cuando RabbitMQ esta deshabilitado (desarrollo local, tests).
/// Al devolver false, el llamador entrega en linea igual que antes de la cola.
/// </summary>
public class NullNotificationQueuePublisher : INotificationQueuePublisher
{
    public Task<bool> PublishAsync(NotificationDeliveryMessage message, CancellationToken ct = default)
        => Task.FromResult(false);
}
