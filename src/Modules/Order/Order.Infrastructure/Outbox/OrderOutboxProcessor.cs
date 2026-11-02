using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Infrastructure.Options;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Outbox;

// Order keeps its public module-specific registration while sharing the same
// lease, ownership, attempt-limit, and retry implementation as other producers.
public sealed class OrderOutboxProcessor
{
    private readonly ModuleOutboxProcessor<OrderDbContext, OrderOutboxPublisherIdentity> _processor;

    public OrderOutboxProcessor(
        IOrderOutboxStore store,
        IIntegrationEventPublisher<OrderOutboxPublisherIdentity> publisher,
        IOptions<OrderOutboxOptions> options,
        TimeProvider clock,
        ILogger<ModuleOutboxProcessor<OrderDbContext, OrderOutboxPublisherIdentity>> logger)
    {
        var value = options.Value;
        _processor = new(store, publisher, Microsoft.Extensions.Options.Options.Create(
            new ModuleOutboxOptions<OrderOutboxPublisherIdentity>
            {
                BatchSize = value.BatchSize,
                ClaimDuration = value.ClaimDuration,
                PollingInterval = value.PollingInterval,
                MaximumAttempts = value.MaximumAttempts,
                InitialRetryDelay = value.InitialRetryDelay,
                MaximumRetryDelay = value.MaximumRetryDelay,
                RetryJitterFactor = value.RetryJitterFactor
            }), clock, logger);
    }

    public Task ProcessBatchAsync(CancellationToken cancellationToken = default)
        => _processor.ProcessBatchAsync(cancellationToken);
}
