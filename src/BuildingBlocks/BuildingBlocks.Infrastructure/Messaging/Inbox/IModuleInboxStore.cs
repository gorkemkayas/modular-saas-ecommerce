using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed record ClaimedInboxMessage(
    Guid Id, Guid LockId, string EventType, string RoutingKey, string Payload,
    DateTime OccurredOnUtc, int AttemptCount);

public interface IModuleInboxStore<TContext> where TContext : DbContext
{
    Task<bool> WriteAsync(IntegrationMessage message, DateTime receivedOnUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimedInboxMessage>> ClaimAsync(Guid lockId, DateTime nowUtc,
        DateTime lockedUntilUtc, int batchSize, CancellationToken cancellationToken = default);
    Task<bool> RenewLeaseAsync(Guid messageId, Guid lockId, DateTime nowUtc,
        DateTime lockedUntilUtc, CancellationToken cancellationToken = default);
    Task<bool> MarkProcessedAsync(Guid messageId, Guid lockId, DateTime processedOnUtc,
        CancellationToken cancellationToken = default);
    Task<bool> ScheduleRetryAsync(Guid messageId, Guid lockId, DateTime nextAttemptAtUtc,
        string error, CancellationToken cancellationToken = default);
    Task<bool> MarkFailedAsync(Guid messageId, Guid lockId, string error,
        CancellationToken cancellationToken = default);
}
