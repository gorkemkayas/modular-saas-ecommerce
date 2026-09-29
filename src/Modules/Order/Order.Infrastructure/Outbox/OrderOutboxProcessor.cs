using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Infrastructure.Options;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Outbox;

public sealed class OrderOutboxProcessor
{
    private readonly IOrderOutboxStore _orderOutboxStore;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly OrderOutboxOptions _orderOutboxOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OrderOutboxProcessor> _logger;

    public OrderOutboxProcessor(
        IOrderOutboxStore orderOutboxStore,
        IIntegrationEventPublisher integrationEventPublisher,
        IOptions<OrderOutboxOptions> orderOutboxOptions,
        TimeProvider timeProvider,
        ILogger<OrderOutboxProcessor> logger)
    {
        _orderOutboxStore = orderOutboxStore;
        _integrationEventPublisher = integrationEventPublisher;
        _orderOutboxOptions = orderOutboxOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var lockId = Guid.NewGuid();
        var lockedUntilUtc = nowUtc.Add(_orderOutboxOptions.ClaimDuration);

        var claimedMessages =
            await _orderOutboxStore.ClaimAsync(
                lockId,
                nowUtc,
                lockedUntilUtc,
                _orderOutboxOptions.BatchSize,
                cancellationToken);

        foreach (var claimedMessage in claimedMessages)
        {
            await ProcessMessageAsync(
                claimedMessage,
                cancellationToken);
        }

    }

    private async Task ProcessMessageAsync(
        ClaimedOutboxMessage claimedOutboxMessage,
        CancellationToken cancellationToken)
    {
        var integrationMessage = new IntegrationMessage(
            claimedOutboxMessage.Id,
            claimedOutboxMessage.EventType,
            claimedOutboxMessage.RoutingKey,
            claimedOutboxMessage.Payload,
            claimedOutboxMessage.OccurredOnUtc);

        try
        {
            await _integrationEventPublisher.PublishAsync(
                integrationMessage,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await HandlePublishingFailureAsync(
                claimedOutboxMessage,
                exception,
                cancellationToken);

            return;
        }

        var publishedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

        var markedAsPublished = await _orderOutboxStore.MarkPublishedAsync(
            claimedOutboxMessage.Id,
            claimedOutboxMessage.LockId,
            publishedAtUtc,
            cancellationToken);

        if (!markedAsPublished)
        {
            _logger.LogWarning(
                "Outbox message {OutboxMessageId} could not be marked " +
                "as published because its processing ownership was lost.",
                claimedOutboxMessage.Id);
        }
    }

    private async Task HandlePublishingFailureAsync(
        ClaimedOutboxMessage claimedOutboxMessage,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Publishing Outbox message {OutboxMessageId} failed " +
            "on attempt {AttemptCount}.",
            claimedOutboxMessage.Id,
            claimedOutboxMessage.AttemptCount);

        var error = exception.ToString();

        if (claimedOutboxMessage.AttemptCount >=
            _orderOutboxOptions.MaximumAttempts)
        {
            var markedAsFailed =
                await _orderOutboxStore.MarkFailedAsync(
                    claimedOutboxMessage.Id,
                    claimedOutboxMessage.LockId,
                    error,
                    cancellationToken);

            if (!markedAsFailed)
            {
                _logger.LogWarning(
                    "Outbox message {OutboxMessageId} could not be marked " +
                    "as failed because its processing ownership was lost.",
                    claimedOutboxMessage.Id);
            }

            return;
        }

        var failureTimeUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        var retryDelay = CalculateRetryDelay(
            claimedOutboxMessage.AttemptCount);

        var nextAttemptAtUtc =
            failureTimeUtc.Add(retryDelay);

        var retryScheduled =
            await _orderOutboxStore.ScheduleRetryAsync(
                claimedOutboxMessage.Id,
                claimedOutboxMessage.LockId,
                nextAttemptAtUtc,
                error,
                cancellationToken);

        if (!retryScheduled)
        {
            _logger.LogWarning(
                "Retry could not be scheduled for Outbox message " +
                "{OutboxMessageId} because its processing ownership was lost.",
                claimedOutboxMessage.Id);
        }
    }

    private TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var exponent = Math.Max(
            0,
            attemptCount - 1);

        var multiplier = Math.Pow(
            2,
            exponent);

        var calculatedMilliseconds =
            _orderOutboxOptions.InitialRetryDelay.TotalMilliseconds *
            multiplier;

        var cappedMilliseconds = Math.Min(
            calculatedMilliseconds,
            _orderOutboxOptions.MaximumRetryDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(
            cappedMilliseconds);
    }
}
