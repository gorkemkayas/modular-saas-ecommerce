using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class ModuleOutboxProcessor<TContext, TIdentity>
    where TContext : DbContext
    where TIdentity : class
{
    private readonly IModuleOutboxStore<TContext> _store;
    private readonly IIntegrationEventPublisher<TIdentity> _integrationEventPublisher;
    private readonly ModuleOutboxOptions<TIdentity> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ModuleOutboxProcessor<TContext, TIdentity>> _logger;

    public ModuleOutboxProcessor(
        IModuleOutboxStore<TContext> store,
        IIntegrationEventPublisher<TIdentity> integrationEventPublisher,
        IOptions<ModuleOutboxOptions<TIdentity>> options,
        TimeProvider timeProvider,
        ILogger<ModuleOutboxProcessor<TContext, TIdentity>> logger)
    {
        _store = store;
        _integrationEventPublisher = integrationEventPublisher;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var lockId = Guid.NewGuid();
        var lockedUntilUtc = nowUtc.Add(_options.ClaimDuration);

        var claimedMessages =
            await _store.ClaimAsync(
                lockId,
                nowUtc,
                lockedUntilUtc,
                _options.BatchSize,
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
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        if (!await _store.RenewLeaseAsync(claimedOutboxMessage.Id, claimedOutboxMessage.LockId,
                nowUtc, nowUtc.Add(_options.ClaimDuration), cancellationToken))
            return;

        if (claimedOutboxMessage.AttemptCount > _options.MaximumAttempts)
        {
            await _store.MarkFailedAsync(claimedOutboxMessage.Id, claimedOutboxMessage.LockId,
                "Outbox claim recovery exhausted the attempt budget.", cancellationToken);
            return;
        }

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

        var markedAsPublished = await _store.MarkPublishedAsync(
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
            _options.MaximumAttempts)
        {
            var markedAsFailed =
                await _store.MarkFailedAsync(
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
            await _store.ScheduleRetryAsync(
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
            _options.InitialRetryDelay.TotalMilliseconds *
            multiplier;

        var cappedMilliseconds = Math.Min(
            calculatedMilliseconds,
            _options.MaximumRetryDelay.TotalMilliseconds);

        double jitter = 1 + ((Random.Shared.NextDouble() * 2 - 1) * _options.RetryJitterFactor);
        return TimeSpan.FromMilliseconds(Math.Clamp(cappedMilliseconds * jitter,
            1, _options.MaximumRetryDelay.TotalMilliseconds));
    }
}
