using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence.Inbox;
using RabbitMQ.Client.Events;
using BuildingBlocks.Messaging.RabbitMQ.Retry;

namespace Notification.Infrastructure.Messaging.RabbitMq;

public sealed class NotificationIntegrationEventConsumer
    : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly NotificationIntegrationEventConsumerOptions _consumerOptions;
    private readonly NotificationIntegrationEventFailureRouter _failureRouter;
    private readonly ILogger<NotificationIntegrationEventConsumer> _logger;

    public NotificationIntegrationEventConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        NotificationIntegrationEventFailureRouter failureRouter,
        IOptions<NotificationIntegrationEventConsumerOptions>
            consumerOptions,
        ILogger<NotificationIntegrationEventConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionProvider);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(rabbitMqOptions);
        ArgumentNullException.ThrowIfNull(consumerOptions);
        ArgumentNullException.ThrowIfNull(failureRouter);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionProvider = connectionProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _consumerOptions = consumerOptions.Value;
        _failureRouter = failureRouter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_consumerOptions.Enabled)
        {
            _logger.LogInformation(
                "Notification integration-event consumer is disabled.");

            return;
        }

        IConnection connection =
            await _connectionProvider.GetConnectionAsync(
                RabbitMqConnectionRole.Consuming,
                stoppingToken);

        await using IChannel channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await NotificationIntegrationEventTopology.DeclareAsync(
            channel,
            _rabbitMqOptions.ExchangeName,
            _consumerOptions.QueueName,
            _consumerOptions.Retry,
            stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _consumerOptions.PrefetchCount,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (_, delivery) =>
            HandleDeliveryAsync(
            channel,
            delivery,
            stoppingToken);

        string consumerTag =
            await channel.BasicConsumeAsync(
                queue: _consumerOptions.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Notification integration-event consumer started. " +
            "Queue: {QueueName}, Prefetch: {PrefetchCount}, " +
            "ConsumerTag: {ConsumerTag}.",
            _consumerOptions.QueueName,
            _consumerOptions.PrefetchCount,
            consumerTag);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Notification integration-event consumer is stopping.");
        }
    }
    private async Task HandleDeliveryAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        string brokerMessageId =
            delivery.BasicProperties.MessageId ?? "<missing>";

        InboxMessage inboxMessage;

        try
        {
            inboxMessage = CreateInboxMessage(delivery);
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(
                exception,
                "RabbitMQ message could not be converted to an inbox message. " +
                "MessageId: {MessageId}, RoutingKey: {RoutingKey}",
                brokerMessageId,
                delivery.RoutingKey);

            await RouteFailureAndAcknowledgeAsync(
                channel,
                delivery,
                isPermanentFailure: true,
                messageId: brokerMessageId,
                cancellationToken);
            return;
        }
        InboxMessageWriteResult writeResult;
        try
        {
            await using AsyncServiceScope scope =
                _serviceScopeFactory.CreateAsyncScope();

            var writer = scope.ServiceProvider
                .GetRequiredService<IInboxMessageWriter>();

            writeResult =
                await writer.WriteAsync(
                    inboxMessage,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Inbox persistence was cancelled during application shutdown. " +
                "MessageId: {MessageId}",
                inboxMessage.Id);

            return;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "RabbitMQ message could not be persisted to the inbox. " +
                "MessageId: {MessageId}",
                inboxMessage.Id);

            await RouteFailureAndAcknowledgeAsync(
                channel,
                delivery,
                isPermanentFailure: false,
                inboxMessage.Id.ToString("D"),
                cancellationToken);

            return;
        }

        try
        {
            await channel.BasicAckAsync(
            deliveryTag: delivery.DeliveryTag,
            multiple: false,
            cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Inbox message was persisted, but ACK was cancelled during shutdown. " +
                "MessageId: {MessageId}",
                inboxMessage.Id);

            return;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Inbox message was persisted, but RabbitMQ ACK failed. " +
                "MessageId: {MessageId}",
                inboxMessage.Id);

            return;
        }

        _logger.LogInformation(
            "RabbitMQ message was persisted and acknowledged. " +
            "MessageId: {MessageId}, WriteResult: {WriteResult}",
            inboxMessage.Id,
            writeResult);
    }
    private static InboxMessage CreateInboxMessage(
        BasicDeliverEventArgs delivery)
    {
        string? messageIdValue =
            delivery.BasicProperties.MessageId;

        if (!Guid.TryParseExact(
                messageIdValue,
                "D",
                out Guid messageId))
        {
            throw new InvalidDataException(
                "RabbitMQ MessageId must be a valid GUID " +
                "in D format.");
        }

        string? eventType =
            delivery.BasicProperties.Type;

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new InvalidDataException(
                "RabbitMQ message type is required.");
        }

        string routingKey = GetOriginalRoutingKey(delivery);
        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new InvalidDataException(
                "RabbitMQ routing key is required.");
        }

        if (!string.Equals(
                delivery.BasicProperties.ContentType,
                "application/json",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "RabbitMQ message content type must be " +
                "application/json.");
        }

        if (!string.Equals(
                delivery.BasicProperties.ContentEncoding,
                "utf-8",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "RabbitMQ message content encoding must be utf-8.");
        }

        string payload =
            Encoding.UTF8.GetString(delivery.Body.Span);

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidDataException(
                "RabbitMQ message payload is required.");
        }

        ValidateJson(payload);

        DateTime occurredOnUtc =
            GetOccurredOnUtc(delivery);

        return new InboxMessage(
            messageId,
            eventType,
            routingKey,
            payload,
            occurredOnUtc,
            DateTime.UtcNow);
    }
    private static void ValidateJson(string payload)
    {
        try
        {
            using JsonDocument document =
                JsonDocument.Parse(payload);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "RabbitMQ message payload must contain valid JSON.",
                exception);
        }
    }
    private static DateTime GetOccurredOnUtc(
    BasicDeliverEventArgs delivery)
    {
        long unixTime =
            delivery.BasicProperties.Timestamp.UnixTime;

        if (unixTime <= 0)
        {
            throw new InvalidDataException(
                "RabbitMQ message timestamp is required.");
        }

        try
        {
            return DateTimeOffset
                .FromUnixTimeSeconds(unixTime)
                .UtcDateTime;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidDataException(
                "RabbitMQ message timestamp is invalid.",
                exception);
        }
    }
    private async Task RouteFailureAndAcknowledgeAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        bool isPermanentFailure,
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (isPermanentFailure)
            {
                await _failureRouter.RoutePermanentFailureAsync(
                    delivery,
                    cancellationToken);
            }
            else
            {
                await _failureRouter.RouteTransientFailureAsync(
                    delivery,
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "RabbitMQ failure routing was cancelled during shutdown. " +
                "The original delivery will be requeued. " +
                "MessageId: {MessageId}",
                messageId);

            await RequeueAfterFailureRoutingErrorAsync(
                channel,
                delivery,
                messageId,
                cancellationToken);

            return;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "RabbitMQ delivery could not be routed to retry or " +
                "dead-letter infrastructure. " +
                "The original delivery remains unacknowledged. " +
                "MessageId: {MessageId}",
                messageId);

            return;
        }

        try
        {
            await channel.BasicAckAsync(
                deliveryTag: delivery.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "The failed delivery was republished, but its original " +
                "ACK was cancelled during shutdown. " +
                "MessageId: {MessageId}",
                messageId);

            return;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "The failed delivery was republished, but its original " +
                "RabbitMQ ACK failed. A duplicate delivery is possible. " +
                "MessageId: {MessageId}",
                messageId);

            return;
        }

        _logger.LogInformation(
            "Failed RabbitMQ delivery was routed and its original " +
            "delivery was acknowledged. MessageId: {MessageId}",
            messageId);
    }

    private async Task RequeueAfterFailureRoutingErrorAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(
                _rabbitMqOptions.NetworkRecoveryInterval,
                cancellationToken);

            await channel.BasicNackAsync(
                deliveryTag: delivery.DeliveryTag,
                multiple: false,
                requeue: true,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "RabbitMQ failure routing could not complete, so the " +
                "original delivery was requeued. MessageId: {MessageId}",
                messageId);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "RabbitMQ delivery requeue was cancelled during shutdown. " +
                "The broker will recover the unacknowledged delivery when " +
                "the channel closes. MessageId: {MessageId}",
                messageId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "RabbitMQ delivery could not be requeued after failure " +
                "routing failed. The delivery remains unacknowledged. " +
                "MessageId: {MessageId}",
                messageId);
        }
    }

    private static string GetOriginalRoutingKey(
        BasicDeliverEventArgs delivery)
    {
        bool hasMetadata =
            RabbitMqRetryMetadataCodec.TryRead(
                delivery.BasicProperties.Headers,
                out RabbitMqRetryMetadata? metadata);

        if (hasMetadata && metadata is not null)
        {
            return metadata.OriginalRoutingKey;
        }

        return delivery.RoutingKey;
    }
}
