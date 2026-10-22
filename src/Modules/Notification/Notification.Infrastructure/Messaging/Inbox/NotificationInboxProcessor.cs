using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class NotificationInboxProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly NotificationInboxOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationInboxProcessor> _logger;

    public NotificationInboxProcessor(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<NotificationInboxOptions> options,
        TimeProvider timeProvider,
        ILogger<NotificationInboxProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(
            serviceScopeFactory);

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ClaimedInboxMessage> claimedMessages =
            await ClaimMessagesAsync(
                cancellationToken);

        foreach (ClaimedInboxMessage message in claimedMessages)
        {
            try
            {
                await ProcessMessageAsync(
                    message,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                try
                {
                    await HandleProcessingFailureAsync(
                        message,
                        exception,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception stateTransitionException)
                {
                    _logger.LogError(
                        stateTransitionException,
                        "The failure state of Inbox message " +
                        "{InboxMessageId} could not be persisted. " +
                        "The message will become claimable after its " +
                        "lease expires.",
                        message.Id);
                }
            }
        }
    }

    private async Task<IReadOnlyList<ClaimedInboxMessage>>
        ClaimMessagesAsync(
            CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope =
            _serviceScopeFactory.CreateAsyncScope();

        INotificationInboxStore inboxStore =
            scope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();

        DateTime nowUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        DateTime lockedUntilUtc =
            nowUtc.Add(_options.ClaimDuration);

        return await inboxStore.ClaimAsync(
            Guid.NewGuid(),
            nowUtc,
            lockedUntilUtc,
            _options.BatchSize,
            cancellationToken);
    }

    private async Task ProcessMessageAsync(
        ClaimedInboxMessage message,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope =
            _serviceScopeFactory.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        INotificationInboxStore inboxStore =
            scope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();

        NotificationInboxMessageHandlerResolver resolver =
            scope.ServiceProvider.GetRequiredService<
                NotificationInboxMessageHandlerResolver>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        INotificationInboxMessageHandler handler =
            resolver.Resolve(message.EventType);

        await handler.HandleAsync(
            message,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        DateTime processedOnUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        bool markedAsProcessed =
            await inboxStore.MarkProcessedAsync(
                message.Id,
                message.LockId,
                processedOnUtc,
                cancellationToken);

        if (!markedAsProcessed)
        {
            _logger.LogWarning(
                "Inbox message {InboxMessageId} could not be " +
                "marked as processed because its processing " +
                "ownership was lost. LockId: {LockId}.",
                message.Id,
                message.LockId);

            await transaction.RollbackAsync(
                cancellationToken);

            return;
        }

        await transaction.CommitAsync(
            cancellationToken);

        _logger.LogInformation(
            "Inbox message {InboxMessageId} with event type " +
            "{EventType} was processed successfully.",
            message.Id,
            message.EventType);
    }

    private async Task HandleProcessingFailureAsync(
        ClaimedInboxMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Processing Inbox message {InboxMessageId} " +
            "with event type {EventType} failed on " +
            "attempt {AttemptCount}.",
            message.Id,
            message.EventType,
            message.AttemptCount);

        await using AsyncServiceScope scope =
            _serviceScopeFactory.CreateAsyncScope();

        INotificationInboxStore inboxStore =
            scope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();

        string error = exception.ToString();

        bool shouldFailPermanently =
            IsPermanentFailure(exception) ||
            message.AttemptCount >= _options.MaximumAttempts;

        if (shouldFailPermanently)
        {
            bool markedAsFailed =
                await inboxStore.MarkFailedAsync(
                    message.Id,
                    message.LockId,
                    error,
                    cancellationToken);

            if (!markedAsFailed)
            {
                _logger.LogWarning(
                    "Inbox message {InboxMessageId} could not be " +
                    "marked as failed because its processing " +
                    "ownership was lost. LockId: {LockId}.",
                    message.Id,
                    message.LockId);

                return;
            }

            _logger.LogError(
                "Inbox message {InboxMessageId} with event type " +
                "{EventType} was marked as failed after " +
                "{AttemptCount} attempt(s).",
                message.Id,
                message.EventType,
                message.AttemptCount);

            return;
        }

        DateTime failureTimeUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        TimeSpan retryDelay =
            CalculateRetryDelay(message.AttemptCount);

        DateTime nextAttemptAtUtc =
            failureTimeUtc.Add(retryDelay);

        bool retryScheduled =
            await inboxStore.ScheduleRetryAsync(
                message.Id,
                message.LockId,
                nextAttemptAtUtc,
                error,
                cancellationToken);

        if (!retryScheduled)
        {
            _logger.LogWarning(
                "Retry could not be scheduled for Inbox message " +
                "{InboxMessageId} because its processing " +
                "ownership was lost. LockId: {LockId}.",
                message.Id,
                message.LockId);

            return;
        }

        _logger.LogWarning(
            "Inbox message {InboxMessageId} was scheduled for " +
            "retry at {NextAttemptAtUtc} after attempt " +
            "{AttemptCount}.",
            message.Id,
            nextAttemptAtUtc,
            message.AttemptCount);
    }

    private static bool IsPermanentFailure(
        Exception exception)
    {
        return exception is
            UnsupportedNotificationInboxEventException
            or InvalidDataException;
    }

    private TimeSpan CalculateRetryDelay(
        int attemptCount)
    {
        int exponent = Math.Max(
            0,
            attemptCount - 1);

        double multiplier = Math.Pow(
            2,
            exponent);

        double calculatedMilliseconds =
            _options.InitialRetryDelay.TotalMilliseconds *
            multiplier;

        double cappedMilliseconds = Math.Min(
            calculatedMilliseconds,
            _options.MaximumRetryDelay.TotalMilliseconds);

        double jitterRange =
            _options.RetryJitterFactor;

        double jitterMultiplier =
            1 +
            ((Random.Shared.NextDouble() * 2) - 1) *
            jitterRange;

        double jitteredMilliseconds =
            cappedMilliseconds *
            jitterMultiplier;

        double finalMilliseconds = Math.Clamp(
            jitteredMilliseconds,
            0,
            _options.MaximumRetryDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(
            finalMilliseconds);
    }
}