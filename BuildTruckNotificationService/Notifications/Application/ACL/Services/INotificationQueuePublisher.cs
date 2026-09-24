namespace BuildTruckNotificationService.Notifications.Application.ACL.Services;

/// <summary>
/// Mensaje encolado para entregar una notificacion por un canal concreto.
/// Solo lleva identificadores: el consumidor recarga la notificacion de la BD,
/// de modo que un mensaje viejo nunca entrega contenido obsoleto.
/// </summary>
public record NotificationDeliveryMessage(int NotificationId, string Channel);

/// <summary>
/// Puerto de publicacion en cola. La implementacion (RabbitMQ) vive en Infrastructure.
/// </summary>
public interface INotificationQueuePublisher
{
    /// <summary>
    /// Encola la entrega. Devuelve false si la cola no esta disponible,
    /// para que el llamador decida si entrega en linea como respaldo.
    /// </summary>
    Task<bool> PublishAsync(NotificationDeliveryMessage message, CancellationToken ct = default);
}
