using System.Text;
using System.Text.Json;
using BuildTruckNotificationService.Notifications.Application.ACL.Services;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Messaging;

/// <summary>
/// Consume las colas de entrega y hace el trabajo lento (SMTP, WebSocket)
/// fuera del ciclo de peticion HTTP.
///
/// Reintentos: se cuenta 'x-delivery-count' via reentrega. Al superar
/// MaxRetries se hace nack sin reencolar, y la cola manda el mensaje a la DLQ.
/// </summary>
public class NotificationDeliveryConsumer : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<NotificationDeliveryConsumer> _logger;

    public NotificationDeliveryConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<NotificationDeliveryConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("RabbitMQ deshabilitado; el consumidor no arranca");
            return;
        }

        // El broker puede tardar mas en levantar que este servicio.
        while (!stoppingToken.IsCancellationRequested)
        {
            var channel = await _connection.CreateChannelAsync(stoppingToken);
            if (channel is not null)
            {
                await ConsumeAsync(channel, stoppingToken);
                return;
            }

            _logger.LogWarning("RabbitMQ no disponible; reintentando en 15s");
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task ConsumeAsync(IChannel channel, CancellationToken ct)
    {
        // Un mensaje a la vez por consumidor: no acaparamos la cola.
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => HandleMessageAsync(channel, ea, ct);

        foreach (var queue in new[] { QueueTopology.EmailQueue, QueueTopology.WebSocketQueue })
        {
            await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: ct);
            _logger.LogInformation("Consumiendo la cola {Queue}", queue);
        }

        // Mantiene vivo el BackgroundService mientras el canal escucha.
        await Task.Delay(Timeout.Infinite, ct);
    }

    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        NotificationDeliveryMessage? message = null;
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            message = JsonSerializer.Deserialize<NotificationDeliveryMessage>(json);
        }
        catch (Exception ex)
        {
            // Un mensaje ilegible nunca se va a poder procesar: directo a la DLQ.
            _logger.LogError(ex, "Mensaje ilegible; enviando a la dead-letter queue");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, ct);
            return;
        }

        if (message is null)
        {
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, ct);
            return;
        }

        try
        {
            await DeliverAsync(message, ct);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, ct);
        }
        catch (Exception ex)
        {
            var attempts = CountAttempts(ea);
            var giveUp = attempts >= _settings.MaxRetries;

            _logger.LogWarning(ex,
                "Fallo la entrega de la notificacion {NotificationId} por {Channel} (intento {Attempt}/{Max}){Action}",
                message.NotificationId, message.Channel, attempts, _settings.MaxRetries,
                giveUp ? " -> dead-letter queue" : " -> reencolando");

            // requeue:false + x-dead-letter-exchange = el mensaje acaba en la DLQ.
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: !giveUp, ct);
        }
    }

    /// <summary>Numero de entregas de este mensaje, contando la actual.</summary>
    private static int CountAttempts(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties.Headers?.TryGetValue("x-delivery-count", out var raw) == true
            && raw is int count)
            return count + 1;

        return ea.Redelivered ? 2 : 1;
    }

    /// <summary>
    /// Recarga la notificacion de la BD (el mensaje solo trae el id) y la entrega.
    /// Cada mensaje corre en su propio scope de DI.
    /// </summary>
    private async Task DeliverAsync(NotificationDeliveryMessage message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var delivery = scope.ServiceProvider.GetRequiredService<INotificationDeliveryService>();

        var notification = await repository.FindByIdAsync(message.NotificationId);
        if (notification is null)
        {
            // Se borro entre publicar y consumir: nada que entregar, no es un error.
            _logger.LogInformation(
                "La notificacion {NotificationId} ya no existe; se descarta el mensaje", message.NotificationId);
            return;
        }

        await delivery.DeliverAsync(notification, NotificationChannel.FromString(message.Channel));
    }
}
