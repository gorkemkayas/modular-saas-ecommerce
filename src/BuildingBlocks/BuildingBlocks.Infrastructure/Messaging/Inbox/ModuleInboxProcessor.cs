using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class ModuleInboxProcessor<TContext, TIdentity>(
    IServiceScopeFactory scopeFactory,
    IOptions<ModuleInboxOptions<TIdentity>> options,
    TimeProvider timeProvider,
    ILogger<ModuleInboxProcessor<TContext, TIdentity>> logger)
    where TContext : DbContext
    where TIdentity : class
{
    private readonly ModuleInboxOptions<TIdentity> _options = options.Value;

    public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ClaimedInboxMessage> messages;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IModuleInboxStore<TContext>>();
            var now = timeProvider.GetUtcNow().UtcDateTime;
            messages = await store.ClaimAsync(Guid.NewGuid(), now,
                now.Add(_options.ClaimDuration), _options.BatchSize, cancellationToken);
        }

        foreach (var message in messages)
        {
            try { await ProcessMessageAsync(message, cancellationToken); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception exception)
            {
                try { await PersistFailureAsync(message, exception, cancellationToken); }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception transitionException)
                {
                    logger.LogError(transitionException,
                        "Inbox failure state could not be saved. Message {MessageId} becomes available after lease expiry.",
                        message.Id);
                }
            }
        }
    }

    private async Task ProcessMessageAsync(ClaimedInboxMessage message, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var store = scope.ServiceProvider.GetRequiredService<IModuleInboxStore<TContext>>();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!await store.RenewLeaseAsync(message.Id, message.LockId, now,
                now.Add(_options.ClaimDuration), cancellationToken))
            return;

        if (message.AttemptCount > _options.MaximumAttempts)
        {
            await store.MarkFailedAsync(message.Id, message.LockId, "Inbox retry budget exhausted.", cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var handlers = scope.ServiceProvider.GetServices<IModuleInboxMessageHandler<TIdentity>>()
            .Where(x => string.Equals(x.EventType, message.EventType, StringComparison.Ordinal)).ToArray();
        if (handlers.Length != 1)
            throw new InvalidDataException($"Expected one Inbox handler for '{message.EventType}', found {handlers.Length}.");

        await handlers[0].HandleAsync(message, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        if (!await store.MarkProcessedAsync(message.Id, message.LockId,
                timeProvider.GetUtcNow().UtcDateTime, cancellationToken))
            throw new InvalidOperationException("Inbox processing ownership was lost before commit.");

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Inbox message {MessageId} ({EventType}) processed by {ModuleIdentity}.",
            message.Id, message.EventType, typeof(TIdentity).Name);
    }

    private async Task PersistFailureAsync(ClaimedInboxMessage message, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Inbox message {MessageId} failed on attempt {AttemptCount}.",
            message.Id, message.AttemptCount);
        await using var scope = scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IModuleInboxStore<TContext>>();
        if (exception is InvalidDataException || message.AttemptCount >= _options.MaximumAttempts)
        {
            await store.MarkFailedAsync(message.Id, message.LockId, exception.ToString(), cancellationToken);
            return;
        }

        double baseDelay = Math.Min(_options.MaximumRetryDelay.TotalMilliseconds,
            _options.InitialRetryDelay.TotalMilliseconds * Math.Pow(2, Math.Min(message.AttemptCount - 1, 30)));
        double jitter = 1 + (Random.Shared.NextDouble() * 2 - 1) * _options.RetryJitterFactor;
        var delay = TimeSpan.FromMilliseconds(Math.Clamp(baseDelay * jitter, 1,
            _options.MaximumRetryDelay.TotalMilliseconds));
        await store.ScheduleRetryAsync(message.Id, message.LockId,
            timeProvider.GetUtcNow().UtcDateTime.Add(delay), exception.ToString(), cancellationToken);
    }
}
