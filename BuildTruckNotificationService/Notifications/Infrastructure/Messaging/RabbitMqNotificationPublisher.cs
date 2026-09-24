using System.Text;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL.Services;
using RabbitMQ.Client;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Messaging;

/// <summary>
/// Publica entregas de notificacion en RabbitMQ.
/// Los mensajes son persistentes: sobreviven a un reinicio del broker.
/// </summary>
public class RabbitMqNotificationPublisher : INotificationQueuePublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(
        RabbitMqConnection connection,
        ILogger<RabbitMqNotificationPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<bool> PublishAsync(NotificationDeliveryMessage message, CancellationToken ct = default)
    {
        await using var channel = await _connection.CreateChannelAsync(ct);
        if (channel is null)
        {
            _logger.LogWarning(
                "RabbitMQ no disponible; no se encolo la notificacion {NotificationId} ({Channel})",
                message.NotificationId, message.Channel);
            return false;
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: QueueTopology.Exchange,
                routingKey: QueueTopology.RoutingKeyFor(message.Channel),
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al publicar la notificacion {NotificationId}", message.NotificationId);
            return false;
        }
    }
}
