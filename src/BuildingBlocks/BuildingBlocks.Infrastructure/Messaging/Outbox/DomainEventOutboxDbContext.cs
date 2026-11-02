using System.Text.Json;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Messaging.Abstractions.Events;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public abstract class DomainEventOutboxDbContext : DbContext
{
    private bool _saving;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected DomainEventOutboxDbContext(DbContextOptions options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected abstract Task StageEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

    protected void StageIntegrationEvent(Guid storeId, IIntegrationEvent integrationEvent)
    {
        if (OutboxMessages.Local.Any(x => x.Id == integrationEvent.Id))
            return;

        OutboxMessages.Add(new OutboxMessage(
            integrationEvent.Id, storeId, integrationEvent.EventType, integrationEvent.EventType,
            JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonOptions),
            integrationEvent.OccurredOnUtc, DateTime.UtcNow));
    }

    public override int SaveChanges() => throw new InvalidOperationException("Use SaveChangesAsync to persist domain events and Outbox atomically.");
    public override int SaveChanges(bool acceptAllChangesOnSuccess) => SaveChanges();
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (_saving)
            throw new InvalidOperationException("Recursive SaveChangesAsync is not supported.");

        _saving = true;
        try
        {
            var dispatched = new HashSet<Guid>();
            for (var batch = 0; ; batch++)
            {
                var events = ChangeTracker.Entries<IHasDomainEvents>()
                    .SelectMany(x => x.Entity.DomainEvents).Where(x => !dispatched.Contains(x.Id)).ToArray();
                if (events.Length == 0) break;
                if (batch >= 10) throw new InvalidOperationException("Domain event dispatch batch limit exceeded.");
                foreach (var domainEvent in events)
                {
                    await StageEventAsync(domainEvent, cancellationToken);
                    dispatched.Add(domainEvent.Id);
                }
            }

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
                entry.Entity.ClearDomainEvents();
            return result;
        }
        finally { _saving = false; }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
