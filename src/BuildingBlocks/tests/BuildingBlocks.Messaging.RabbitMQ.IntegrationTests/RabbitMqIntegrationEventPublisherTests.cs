using System.Text;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Publishing;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqIntegrationEventPublisherTests
{
    [TestMethod]
    public async Task PublishAsync_WhenMessageIsRoutable_PublishesBodyAndMetadata()
    {
        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        const string routingKey = "order.order-cancelled.v1";
        RabbitMqOptions options = RabbitMqTestContainer.CreateOptions(exchangeName);

        await using IConnection consumerConnection =
            await RabbitMqTestContainer.CreateConnectionAsync();
        await using IChannel consumerChannel =
            await consumerConnection.CreateChannelAsync();

        await consumerChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false);

        await consumerChannel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: null,
            passive: false,
            noWait: false);

        await consumerChannel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            arguments: null,
            noWait: false);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(OptionsFactory.Create(options));
        await using var publisher =
            new RabbitMqIntegrationEventPublisher(
                connectionProvider,
                OptionsFactory.Create(options));

        var occurredOnUtc = new DateTime(
            2026,
            10,
            1,
            9,
            30,
            0,
            DateTimeKind.Utc);
        var message = new IntegrationMessage(
            Guid.NewGuid(),
            routingKey,
            routingKey,
            "{\"orderId\":\"9dc66dfa-4298-47ad-88ee-e874280fe923\"}",
            occurredOnUtc);

        await publisher.PublishAsync(message);

        BasicGetResult? delivery = await WaitForMessageAsync(
            consumerChannel,
            queueName,
            TimeSpan.FromSeconds(5));

        Assert.IsNotNull(delivery);
        Assert.AreEqual(message.Payload, Encoding.UTF8.GetString(delivery.Body.Span));
        Assert.AreEqual(message.Id.ToString("D"), delivery.BasicProperties.MessageId);
        Assert.AreEqual(message.EventType, delivery.BasicProperties.Type);
        Assert.AreEqual("application/json", delivery.BasicProperties.ContentType);
        Assert.AreEqual("utf-8", delivery.BasicProperties.ContentEncoding);
        Assert.AreEqual(DeliveryModes.Persistent, delivery.BasicProperties.DeliveryMode);
        Assert.AreEqual(
            new DateTimeOffset(occurredOnUtc).ToUnixTimeSeconds(),
            delivery.BasicProperties.Timestamp.UnixTime);
        Assert.AreEqual(routingKey, delivery.RoutingKey);

        await consumerChannel.BasicAckAsync(
            delivery.DeliveryTag,
            multiple: false);
    }

    [TestMethod]
    public async Task PublishAsync_WhenMessageIsUnroutable_ThrowsReturnedPublishException()
    {
        string exchangeName = CreateUniqueName("exchange");
        RabbitMqOptions options = RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(OptionsFactory.Create(options));
        await using var publisher =
            new RabbitMqIntegrationEventPublisher(
                connectionProvider,
                OptionsFactory.Create(options));

        var message = new IntegrationMessage(
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            CreateUniqueName("unroutable"),
            "{}",
            DateTime.UtcNow);

        PublishReturnException exception =
            await Assert.ThrowsExactlyAsync<PublishReturnException>(
                () => publisher.PublishAsync(message));

        Assert.IsTrue(exception.IsReturn);
    }

    [TestMethod]
    public async Task PublishAsync_WhenBrokerIsUnavailable_PropagatesConnectionFailure()
    {
        var options = new RabbitMqOptions
        {
            HostName = "127.0.0.1",
            Port = 1,
            UserName = "unused",
            Password = "unused",
            VirtualHost = "/",
            ClientProvidedName = "unavailable-broker-test",
            ExchangeName = CreateUniqueName("exchange"),
            RequestedHeartbeat = TimeSpan.FromSeconds(1),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(1),
            ConnectionTimeout = TimeSpan.FromSeconds(1),
            PublishTimeout = TimeSpan.FromSeconds(2)
        };

        await using var connectionProvider =
            new RabbitMqConnectionProvider(OptionsFactory.Create(options));
        await using var publisher =
            new RabbitMqIntegrationEventPublisher(
                connectionProvider,
                OptionsFactory.Create(options));

        var message = new IntegrationMessage(
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            "{}",
            DateTime.UtcNow);

        await Assert.ThrowsExactlyAsync<BrokerUnreachableException>(
            () => publisher.PublishAsync(message));
    }

    [TestMethod]
    public async Task PublishAsync_WhenConnectionCreationStalls_StopsAtPublishTimeout()
    {
        var options = new RabbitMqOptions
        {
            ExchangeName = CreateUniqueName("exchange"),
            PublishTimeout = TimeSpan.FromMilliseconds(100)
        };

        await using var connectionProvider =
            new HangingConnectionProvider();
        await using var publisher =
            new RabbitMqIntegrationEventPublisher(
                connectionProvider,
                OptionsFactory.Create(options));

        var message = new IntegrationMessage(
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            "{}",
            DateTime.UtcNow);

        OperationCanceledException? timeoutException = null;

        try
        {
            await publisher.PublishAsync(message);
        }
        catch (OperationCanceledException exception)
        {
            timeoutException = exception;
        }

        Assert.IsNotNull(timeoutException);
    }

    private static async Task<BasicGetResult?> WaitForMessageAsync(
        IChannel channel,
        string queueName,
        TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);

        while (!timeoutSource.IsCancellationRequested)
        {
            BasicGetResult? result = await channel.BasicGetAsync(
                queueName,
                autoAck: false,
                timeoutSource.Token);

            if (result is not null)
            {
                return result;
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(50),
                timeoutSource.Token);
        }

        return null;
    }

    private static string CreateUniqueName(string prefix)
    {
        return $"{prefix}.{Guid.NewGuid():N}";
    }

    private sealed class HangingConnectionProvider : IRabbitMqConnectionProvider
    {
        public async ValueTask<IConnection> GetConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);

            throw new InvalidOperationException(
                "The simulated connection operation should be cancelled.");
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
