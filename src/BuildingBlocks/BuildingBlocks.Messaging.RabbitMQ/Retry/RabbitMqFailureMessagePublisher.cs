using System;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public sealed class RabbitMqFailureMessagePublisher<TPublisherIdentity> 
    : IRabbitMqFailureMessagePublisher<TPublisherIdentity>,
      IAsyncDisposable
    where TPublisherIdentity : class
{
    private static readonly CreateChannelOptions ChannelOptions =
        new(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );

    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly SemaphoreSlim _publishGate = new(1, 1);
    private readonly TimeSpan _publishTimeout;

    private IChannel? _channel;
    private bool _disposed;

    public RabbitMqFailureMessagePublisher(
        IRabbitMqConnectionProvider connectionProvider,
        IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(connectionProvider);
        ArgumentNullException.ThrowIfNull(options);

        _connectionProvider = connectionProvider;
        _publishTimeout = options.Value.PublishTimeout;
    }

    public async Task PublishAsync(string destinationExchangeName,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties sourceProperties,
        RabbitMqRetryMetadata retryMetadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationExchangeName);
        ArgumentNullException.ThrowIfNull(routingKey);
        ArgumentNullException.ThrowIfNull(sourceProperties);
        ArgumentNullException.ThrowIfNull(retryMetadata);

        using var publishTimeoutSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);

        publishTimeoutSource.CancelAfter(_publishTimeout);

        await _publishGate.WaitAsync(publishTimeoutSource.Token);

        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            IChannel channel = await GetOrCreateChannelAsync(publishTimeoutSource.Token);
            BasicProperties properties = CreateBasicProperties(sourceProperties, retryMetadata);

            await channel.BasicPublishAsync(
                exchange: destinationExchangeName,
                routingKey: routingKey,
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
        CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            IChannel closedChannel = _channel;
            _channel = null;

            await closedChannel.DisposeAsync();
        }

        IConnection connection =
            await _connectionProvider.GetConnectionAsync(
                RabbitMqConnectionRole.Publishing,
                cancellationToken);

        _channel = await connection.CreateChannelAsync(
            ChannelOptions,
            cancellationToken);

        return _channel;
    }
    private static BasicProperties CreateBasicProperties(
        IReadOnlyBasicProperties sourceProperties,
        RabbitMqRetryMetadata retryMetadata)
    {
        Dictionary<string, object?> headers =
            sourceProperties.Headers is null
                ? []
                : new Dictionary<string, object?>(
                    sourceProperties.Headers);

        RabbitMqRetryMetadataCodec.Write(
            headers,
            retryMetadata);

        return new BasicProperties
        {
            AppId = sourceProperties.AppId,
            ContentEncoding = sourceProperties.ContentEncoding,
            ContentType = sourceProperties.ContentType,
            CorrelationId = sourceProperties.CorrelationId,
            DeliveryMode = DeliveryModes.Persistent,
            Headers = headers,
            MessageId = sourceProperties.MessageId,
            Priority = sourceProperties.Priority,
            ReplyTo = sourceProperties.ReplyTo,
            Timestamp = sourceProperties.Timestamp,
            Type = sourceProperties.Type,
            UserId = sourceProperties.UserId
        };
    }
}
