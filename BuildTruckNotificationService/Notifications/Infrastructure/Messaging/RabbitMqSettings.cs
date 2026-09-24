namespace BuildTruckNotificationService.Notifications.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "buildtruck";
    public string Password { get; set; } = string.Empty;

    /// <summary>Reintentos antes de mandar el mensaje a la dead-letter queue.</summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Nombres de la topologia. Un exchange directo enruta por canal
/// (EMAIL, WEBSOCKET) a su cola; los fallos van a la DLQ.
/// </summary>
public static class QueueTopology
{
    public const string Exchange = "buildtruck.notifications";
    public const string DeadLetterExchange = "buildtruck.notifications.dlx";

    public const string EmailQueue = "notifications.email";
    public const string WebSocketQueue = "notifications.websocket";
    public const string DeadLetterQueue = "notifications.dead-letter";

    public const string EmailRoutingKey = "EMAIL";
    public const string WebSocketRoutingKey = "WEBSOCKET";

    public static string RoutingKeyFor(string channel) => channel.ToUpperInvariant();

    public static string? QueueFor(string channel) => channel.ToUpperInvariant() switch
    {
        EmailRoutingKey => EmailQueue,
        WebSocketRoutingKey => WebSocketQueue,
        _ => null
    };
}
