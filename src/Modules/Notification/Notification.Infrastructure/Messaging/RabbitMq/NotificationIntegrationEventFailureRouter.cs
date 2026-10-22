using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Options;
using RabbitMQ.Client.Events;

namespace Notification.Infrastructure.Messaging.RabbitMq;

public sealed class NotificationIntegrationEventFailureRouter
{
    private readonly IRabbitMqFailureMessagePublisher<NotificationFailurePublisherIdentity> _failurePublisher;
    private readonly NotificationIntegrationEventConsumerOptions _consumerOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationIntegrationEventFailureRouter> _logger;

    public NotificationIntegrationEventFailureRouter(
        IRabbitMqFailureMessagePublisher<NotificationFailurePublisherIdentity> failurePublisher,
        IOptions<NotificationIntegrationEventConsumerOptions> consumerOptions,
        TimeProvider timeProvider,
        ILogger<NotificationIntegrationEventFailureRouter> logger
    )
    {
        ArgumentNullException.ThrowIfNull(failurePublisher);
        ArgumentNullException.ThrowIfNull(consumerOptions);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _failurePublisher = failurePublisher;
        _consumerOptions = consumerOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RouteTransientFailureAsync(
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        RabbitMqRetryMetadata currentMetadata = ReadOrCreateMetadata(eventArgs);

        int nextRetryAttempt = checked(currentMetadata.RetryAttempt + 1);

        RabbitMqRetryMetadata nextMetadata = currentMetadata with
        {
            RetryAttempt = nextRetryAttempt
        };

        bool canRetry =
            nextRetryAttempt <= _consumerOptions.Retry.Delays.Length;

        if (canRetry)
        {
            await PublishToRetryExchangeAsync(
                eventArgs,
                nextMetadata,
                cancellationToken);

            _logger.LogWarning(
                "RabbitMQ delivery was routed for retry. " +
                "MessageId: {MessageId}, RetryAttempt: {RetryAttempt}.",
                eventArgs.BasicProperties.MessageId,
                nextRetryAttempt);

            return;
        }

        await PublishToDeadLetterExchangeAsync(
            eventArgs,
            nextMetadata,
            cancellationToken);

        _logger.LogError(
            "RabbitMQ delivery exhausted all retry attempts " +
            "and was routed to the dead-letter exchange. " +
            "MessageId: {MessageId}, RetryAttempt: {RetryAttempt}.",
            eventArgs.BasicProperties.MessageId,
            nextRetryAttempt);
    }

    public async Task RoutePermanentFailureAsync(
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        RabbitMqRetryMetadata currentMetadata;

        try
        {
            currentMetadata = ReadOrCreateMetadata(eventArgs);
        }
        catch (InvalidDataException exception)
        {
            _logger.LogWarning(
                exception,
                "RabbitMQ retry metadata is invalid. It will be replaced " +
                "before the delivery is routed to the dead-letter exchange. " +
                "MessageId: {MessageId}",
                eventArgs.BasicProperties.MessageId);

            currentMetadata = CreateInitialMetadata(eventArgs);
        }

        await PublishToDeadLetterExchangeAsync(
            eventArgs,
            currentMetadata,
            cancellationToken);
    }

    private RabbitMqRetryMetadata ReadOrCreateMetadata(BasicDeliverEventArgs eventArgs)
    {
        bool hasMetadata = RabbitMqRetryMetadataCodec.TryRead(
            eventArgs.BasicProperties.Headers,
            out RabbitMqRetryMetadata? metadata);

        if (hasMetadata && metadata is not null)
        {
            return metadata;
        }

        return CreateInitialMetadata(eventArgs);
    }

    private RabbitMqRetryMetadata CreateInitialMetadata(
        BasicDeliverEventArgs eventArgs)
    {
        return new RabbitMqRetryMetadata(
            RetryAttempt: 0,
            OriginalExchange: eventArgs.Exchange,
            OriginalRoutingKey: eventArgs.RoutingKey,
            FirstFailedOnUtc: _timeProvider.GetUtcNow().UtcDateTime);
    }

    private async Task PublishToRetryExchangeAsync(
        BasicDeliverEventArgs eventArgs,
        RabbitMqRetryMetadata nextMetadata,
        CancellationToken cancellationToken)
    {
        string retryExchangeName =
            NotificationIntegrationEventTopology.GetRetryExchangeName(
                _consumerOptions.QueueName);

        await _failurePublisher.PublishAsync(
            destinationExchangeName: retryExchangeName,
            routingKey: string.Empty,
            body: eventArgs.Body,
            sourceProperties: eventArgs.BasicProperties,
            retryMetadata: nextMetadata,
            cancellationToken: cancellationToken);
    }

    private async Task PublishToDeadLetterExchangeAsync(
        BasicDeliverEventArgs eventArgs,
        RabbitMqRetryMetadata nextMetadata,
        CancellationToken cancellationToken)
    {
        string deadLetterExchangeName =
            NotificationIntegrationEventTopology.GetDeadLetterExchangeName(
                _consumerOptions.QueueName);

        await _failurePublisher.PublishAsync(
            destinationExchangeName: deadLetterExchangeName,
            routingKey: string.Empty,
            body: eventArgs.Body,
            sourceProperties: eventArgs.BasicProperties,
            retryMetadata: nextMetadata,
            cancellationToken: cancellationToken);
    }
}
