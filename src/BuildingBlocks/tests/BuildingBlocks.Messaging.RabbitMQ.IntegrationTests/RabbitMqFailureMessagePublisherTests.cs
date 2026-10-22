using System.Text;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using RabbitMQ.Client;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqFailureMessagePublisherTests
{
    [TestMethod]
    public async Task PublishAsync_WhenRetryRouteExists_PreservesMessageAndWritesRetryMetadata()
    {
        string exchangeName = CreateUniqueName("retry-exchange");
        string queueName = CreateUniqueName("retry-queue");
        RabbitMqOptions options =
            RabbitMqTestContainer.CreateOptions(
                CreateUniqueName("events-exchange"));

        await using IConnection consumerConnection =
            await RabbitMqTestContainer.CreateConnectionAsync();
        await using IChannel consumerChannel =
            await consumerConnection.CreateChannelAsync();

        await consumerChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Headers,
            durable: false,
            autoDelete: true,
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
            routingKey: string.Empty,
            arguments: new Dictionary<string, object?>
            {
                ["x-match"] = "all",
                [RabbitMqRetryHeaders.RetryAttempt] = 1
            },
            noWait: false);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(options));
        await using var publisher =
            new RabbitMqFailureMessagePublisher<
                TestFailurePublisherIdentity>(
                    connectionProvider,
                    OptionsFactory.Create(options));

        Guid messageId = Guid.NewGuid();
        byte[] body = Encoding.UTF8.GetBytes(
            "{\"orderId\":\"ef539974-4fb9-49f5-aa55-75dbbeaf7940\"}");
        var sourceProperties = new BasicProperties
        {
            MessageId = messageId.ToString("D"),
            Type = "order.order-cancelled.v1",
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            CorrelationId = "correlation-123",
            DeliveryMode = DeliveryModes.Persistent,
            Expiration = "1000",
            Headers = new Dictionary<string, object?>
            {
                ["x-trace-id"] = Encoding.UTF8.GetBytes("trace-123")
            },
            Timestamp = new AmqpTimestamp(1_798_800_000)
        };
        var metadata = new RabbitMqRetryMetadata(
            RetryAttempt: 1,
            OriginalExchange: "ecommerce.events",
            OriginalRoutingKey: "order.order-cancelled.v1",
            FirstFailedOnUtc: new DateTime(
                2027,
                1,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc));

        await publisher.PublishAsync(
            exchangeName,
            string.Empty,
            body,
            sourceProperties,
            metadata);

        BasicGetResult delivery =
            await WaitForMessageAsync(
                consumerChannel,
                queueName,
                TimeSpan.FromSeconds(5));

        Assert.AreEqual(
            Encoding.UTF8.GetString(body),
            Encoding.UTF8.GetString(delivery.Body.Span));
        Assert.AreEqual(
            sourceProperties.MessageId,
            delivery.BasicProperties.MessageId);
        Assert.AreEqual(
            sourceProperties.Type,
            delivery.BasicProperties.Type);
        Assert.AreEqual(
            sourceProperties.CorrelationId,
            delivery.BasicProperties.CorrelationId);
        Assert.AreEqual(
            DeliveryModes.Persistent,
            delivery.BasicProperties.DeliveryMode);
        Assert.IsFalse(
            sourceProperties.Headers.ContainsKey(
                RabbitMqRetryHeaders.RetryAttempt),
            "The source properties were mutated.");

        RabbitMqRetryMetadata deliveredMetadata =
            RabbitMqRetryMetadataCodec.Read(
                delivery.BasicProperties.Headers!);

        Assert.AreEqual(metadata, deliveredMetadata);

        await consumerChannel.BasicAckAsync(
            delivery.DeliveryTag,
            multiple: false);
    }

    [TestMethod]
    public async Task PublishAsync_WhenDestinationExchangeIsBlank_RejectsArgument()
    {
        RabbitMqOptions options =
            RabbitMqTestContainer.CreateOptions(
                CreateUniqueName("events-exchange"));

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(options));
        await using var publisher =
            new RabbitMqFailureMessagePublisher<
                TestFailurePublisherIdentity>(
                    connectionProvider,
                    OptionsFactory.Create(options));

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => publisher.PublishAsync(
                " ",
                string.Empty,
                ReadOnlyMemory<byte>.Empty,
                new BasicProperties(),
                new RabbitMqRetryMetadata(
                    1,
                    "ecommerce.events",
                    "order.order-cancelled.v1",
                    DateTime.UtcNow)));
    }

    private static async Task<BasicGetResult> WaitForMessageAsync(
        IChannel channel,
        string queueName,
        TimeSpan timeout)
    {
        using var timeoutSource =
            new CancellationTokenSource(timeout);

        while (true)
        {
            BasicGetResult? delivery =
                await channel.BasicGetAsync(
                    queueName,
                    autoAck: false,
                    timeoutSource.Token);

            if (delivery is not null)
            {
                return delivery;
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(50),
                timeoutSource.Token);
        }
    }

    private static string CreateUniqueName(string prefix)
    {
        return $"{prefix}.{Guid.NewGuid():N}";
    }

    private sealed class TestFailurePublisherIdentity
    {
    }
}
