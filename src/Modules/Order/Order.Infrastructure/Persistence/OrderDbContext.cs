using BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Events;
using Order.Infrastructure.Persistence.Outbox;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext, IUnitOfWork
{
    private const int MaxDomainEventDispatchBatches = 10;

    private readonly IOrderDomainEventDispatcher _domainEventDispatcher;
    private bool _isSavingChanges;
    public OrderDbContext(DbContextOptions<OrderDbContext> options, IOrderDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<Order.Domain.Entities.OrderItem> OrderItems => Set<Order.Domain.Entities.OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public override int SaveChanges()
    {
        throw new InvalidOperationException(
            "OrderDbContext requires SaveChangesAsync so domain events " +
            "can be dispatched before persistence.");
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new InvalidOperationException(
            "OrderDbContext requires SaveChangesAsync so domain events " +
            "can be dispatched before persistence.");
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(
            acceptAllChangesOnSuccess: true,
            cancellationToken: cancellationToken);
    }
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (_isSavingChanges)
        {
            throw new InvalidOperationException(
                "Recursive SaveChangesAsync calls are not allowed while " +
                "Order domain events are being dispatched.");
        }

        _isSavingChanges = true;

        try
        {
            var dispatchedEventIds = new HashSet<Guid>();
            var dispatchedBatchCount = 0;

            while (true)
            {
                var domainEvents = ChangeTracker
                    .Entries<IHasDomainEvents>()
                    .SelectMany(entry => entry.Entity.DomainEvents)
                    .Where(domainEvent =>
                        !dispatchedEventIds.Contains(domainEvent.Id))
                    .ToArray();

                if (domainEvents.Length == 0)
                {
                    break;
                }

                if (dispatchedBatchCount >=
                    MaxDomainEventDispatchBatches)
                {
                    throw new InvalidOperationException(
                        "Order domain event dispatch exceeded the maximum " +
                        $"of {MaxDomainEventDispatchBatches} batches.");
                }

                await _domainEventDispatcher.DispatchAsync(
                    domainEvents,
                    cancellationToken);

                foreach (var domainEvent in domainEvents)
                {
                    dispatchedEventIds.Add(domainEvent.Id);
                }

                dispatchedBatchCount++;
            }

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            var aggregates = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .ToArray();

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            return result;
        }
        finally
        {
            _isSavingChanges = false;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
