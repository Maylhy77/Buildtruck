using BuildTruckNotificationService.Notifications.Application.ACL.Services;
using BuildTruckNotificationService.Notifications.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Xunit;

namespace BuildTruckBackend.Tests;

/// <summary>
/// Verifica la topologia y la publicacion contra un RabbitMQ real.
/// Se salta si RABBITMQ_TEST_HOST no esta definido.
/// </summary>
public class RabbitMqIntegrationTests
{
    private static string? Host => Environment.GetEnvironmentVariable("RABBITMQ_TEST_HOST");
    private static int Port => int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_TEST_PORT"), out var p) ? p : 5672;

    private static RabbitMqConnection CreateConnection(string host, int port)
    {
        var settings = Options.Create(new RabbitMqSettings
        {
            Enabled = true,
            Host = host,
            Port = port,
            Username = "buildtruck",
            Password = "testpass"
        });

        return new RabbitMqConnection(settings, NullLogger<RabbitMqConnection>.Instance);
    }

    [SkippableFact]
    public async Task DeclaresTopology_WithQueuesBoundToExchange()
    {
        Skip.If(string.IsNullOrEmpty(Host), "RABBITMQ_TEST_HOST no definido");
        await using var conn = CreateConnection(Host!, Port);

        var channel = await conn.CreateChannelAsync();
        Assert.NotNull(channel);

        // QueueDeclarePassive falla si la cola no existe: prueba que la topologia se creo.
        foreach (var queue in new[]
                 {
                     QueueTopology.EmailQueue,
                     QueueTopology.WebSocketQueue,
                     QueueTopology.DeadLetterQueue
                 })
        {
            var ok = await channel!.QueueDeclarePassiveAsync(queue);
            Assert.Equal(queue, ok.QueueName);
        }

        await channel!.DisposeAsync();
    }

    [SkippableFact]
    public async Task PublishesMessage_AndItLandsInTheEmailQueue()
    {
        Skip.If(string.IsNullOrEmpty(Host), "RABBITMQ_TEST_HOST no definido");
        await using var conn = CreateConnection(Host!, Port);

        var publisher = new RabbitMqNotificationPublisher(
            conn, NullLogger<RabbitMqNotificationPublisher>.Instance);

        // Cola limpia antes de publicar.
        var setup = await conn.CreateChannelAsync();
        await setup!.QueuePurgeAsync(QueueTopology.EmailQueue);
        await setup.DisposeAsync();

        var published = await publisher.PublishAsync(new NotificationDeliveryMessage(4242, "EMAIL"));
        Assert.True(published);

        // El mensaje debe estar esperando en la cola de email.
        var channel = await conn.CreateChannelAsync();
        var result = await channel!.BasicGetAsync(QueueTopology.EmailQueue, autoAck: true);

        Assert.NotNull(result);
        var json = System.Text.Encoding.UTF8.GetString(result!.Body.ToArray());
        Assert.Contains("4242", json);
        Assert.Contains("EMAIL", json);

        // Persistente: sobrevive a un reinicio del broker.
        Assert.True(result.BasicProperties.Persistent);

        await channel.DisposeAsync();
    }

    [SkippableFact]
    public async Task RoutesToWebSocketQueue_NotEmailQueue()
    {
        Skip.If(string.IsNullOrEmpty(Host), "RABBITMQ_TEST_HOST no definido");
        await using var conn = CreateConnection(Host!, Port);

        var publisher = new RabbitMqNotificationPublisher(
            conn, NullLogger<RabbitMqNotificationPublisher>.Instance);

        var setup = await conn.CreateChannelAsync();
        await setup!.QueuePurgeAsync(QueueTopology.EmailQueue);
        await setup.QueuePurgeAsync(QueueTopology.WebSocketQueue);
        await setup.DisposeAsync();

        await publisher.PublishAsync(new NotificationDeliveryMessage(7, "WEBSOCKET"));

        var channel = await conn.CreateChannelAsync();
        var inWebSocket = await channel!.BasicGetAsync(QueueTopology.WebSocketQueue, autoAck: true);
        var inEmail = await channel.BasicGetAsync(QueueTopology.EmailQueue, autoAck: true);

        Assert.NotNull(inWebSocket);   // llego a su cola
        Assert.Null(inEmail);          // y no ensucio la otra

        await channel.DisposeAsync();
    }

    [SkippableFact]
    public async Task RejectedMessage_LandsInDeadLetterQueue()
    {
        Skip.If(string.IsNullOrEmpty(Host), "RABBITMQ_TEST_HOST no definido");
        await using var conn = CreateConnection(Host!, Port);

        var publisher = new RabbitMqNotificationPublisher(
            conn, NullLogger<RabbitMqNotificationPublisher>.Instance);

        var setup = await conn.CreateChannelAsync();
        await setup!.QueuePurgeAsync(QueueTopology.EmailQueue);
        await setup.QueuePurgeAsync(QueueTopology.DeadLetterQueue);
        await setup.DisposeAsync();

        await publisher.PublishAsync(new NotificationDeliveryMessage(999, "EMAIL"));

        // Simula al consumidor agotando los reintentos: nack sin reencolar.
        var channel = await conn.CreateChannelAsync();
        var msg = await channel!.BasicGetAsync(QueueTopology.EmailQueue, autoAck: false);
        Assert.NotNull(msg);
        await channel.BasicNackAsync(msg!.DeliveryTag, multiple: false, requeue: false);

        // El x-dead-letter-exchange de la cola lo deposita en la DLQ.
        await Task.Delay(500);
        var dead = await channel.BasicGetAsync(QueueTopology.DeadLetterQueue, autoAck: true);

        Assert.NotNull(dead);
        var json = System.Text.Encoding.UTF8.GetString(dead!.Body.ToArray());
        Assert.Contains("999", json); // el mensaje se conservo, no se perdio

        await channel.DisposeAsync();
    }

    [Fact]
    public async Task PublishReturnsFalse_WhenBrokerIsUnreachable()
    {
        // Puerto cerrado a proposito.
        await using var conn = CreateConnection("127.0.0.1", 5699);
        var publisher = new RabbitMqNotificationPublisher(
            conn, NullLogger<RabbitMqNotificationPublisher>.Instance);

        var published = await publisher.PublishAsync(new NotificationDeliveryMessage(1, "EMAIL"));

        Assert.False(published); // el llamador entregara en linea
    }
}
