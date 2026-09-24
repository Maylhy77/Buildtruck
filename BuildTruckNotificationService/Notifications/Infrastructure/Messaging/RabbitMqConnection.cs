using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Messaging;

/// <summary>
/// Conexion compartida a RabbitMQ. Se conecta de forma perezosa y declara
/// la topologia (exchanges, colas y DLQ) la primera vez que se usa.
/// Si el broker no responde, no tumba el arranque del servicio.
/// </summary>
public class RabbitMqConnection : IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnection> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>Canal listo para usar, o null si el broker no esta disponible.</summary>
    public async Task<IChannel?> CreateChannelAsync(CancellationToken ct = default)
    {
        var connection = await EnsureConnectionAsync(ct);
        if (connection is null) return null;

        try
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: ct);
            await DeclareTopologyAsync(channel, ct);
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo abrir un canal de RabbitMQ");
            return null;
        }
    }

    private async Task<IConnection?> EnsureConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true }) return _connection;

        await _gate.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true }) return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(ct);
            _logger.LogInformation("Conectado a RabbitMQ en {Host}:{Port}", _settings.Host, _settings.Port);
            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ no disponible en {Host}:{Port}", _settings.Host, _settings.Port);
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Exchange directo -> colas por canal. Cada cola manda sus rechazos a la DLX,
    /// que los deposita en la dead-letter queue para inspeccion manual.
    /// </summary>
    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            QueueTopology.Exchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            QueueTopology.DeadLetterExchange, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: ct);

        await channel.QueueDeclareAsync(
            QueueTopology.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(
            QueueTopology.DeadLetterQueue, QueueTopology.DeadLetterExchange, routingKey: "", cancellationToken: ct);

        var withDlx = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = QueueTopology.DeadLetterExchange
        };

        foreach (var (queue, routingKey) in new[]
                 {
                     (QueueTopology.EmailQueue, QueueTopology.EmailRoutingKey),
                     (QueueTopology.WebSocketQueue, QueueTopology.WebSocketRoutingKey)
                 })
        {
            await channel.QueueDeclareAsync(
                queue, durable: true, exclusive: false, autoDelete: false, arguments: withDlx, cancellationToken: ct);
            await channel.QueueBindAsync(queue, QueueTopology.Exchange, routingKey, cancellationToken: ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
