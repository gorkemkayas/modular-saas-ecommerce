using System.Text;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Publishing;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private static readonly CreateChannelOptions ChannelOptions =
    new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly IRabbitMqConnectionProvider _connectionProvider;

    private readonly SemaphoreSlim _publishGate = new(1, 1);

    private readonly string _exchangeName;
    private readonly TimeSpan _publishTimeout;

    private IChannel? _channel;
    private bool _disposed;

    public RabbitMqIntegrationEventPublisher(
        IRabbitMqConnectionProvider connectionProvider,
        IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(connectionProvider);
        ArgumentNullException.ThrowIfNull(options);

        _connectionProvider = connectionProvider;

        RabbitMqOptions rabbitMqOptions = options.Value;
        _exchangeName = rabbitMqOptions.ExchangeName;
        _publishTimeout = rabbitMqOptions.PublishTimeout;
    }

    public async Task PublishAsync(IntegrationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using var publishTimeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        publishTimeoutSource.CancelAfter(_publishTimeout);

        await _publishGate.WaitAsync(publishTimeoutSource.Token);

        try
        {
            ObjectDisposedException.ThrowIf(
                _disposed,
                this);

            IChannel channel = await GetOrCreateChannelAsync(
                publishTimeoutSource.Token);

            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(message.Payload);

            BasicProperties properties = CreateBasicProperties(message);

            await channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: message.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: publishTimeoutSource.Token);
        }
        finally
        {
            _publishGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _publishGate.WaitAsync();

        try
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            IChannel? channel = _channel;
            _channel = null;

            if (channel is not null)
            {
                await channel.DisposeAsync();
            }
        }
        finally
        {
            _publishGate.Release();
        }
    }

    private async ValueTask<IChannel> GetOrCreateChannelAsync(
        CancellationToken cancellationToken = default)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            IChannel channel = _channel;
            _channel = null;
            await channel.DisposeAsync();
        }

        IConnection connection = await _connectionProvider
                                    .GetConnectionAsync(cancellationToken);

        _channel = await connection.CreateChannelAsync(
            ChannelOptions,
            cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        return _channel;
    }

    private static BasicProperties CreateBasicProperties(
        IntegrationMessage message)
    {
        DateTime occurredOnUtc = NormalizeUtc(message.OccurredOnUtc);

        return new BasicProperties
        {
            MessageId = message.Id.ToString("D"),
            Type = message.EventType,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(new DateTimeOffset(occurredOnUtc).ToUnixTimeSeconds())
        };
    }

    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(
                nameof(value))
        };
    }
}
