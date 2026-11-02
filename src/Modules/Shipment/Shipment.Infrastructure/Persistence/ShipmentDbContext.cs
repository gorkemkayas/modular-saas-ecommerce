using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using Shipment.Domain.Events;
using Shipment.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Shipment.Application.Abstractions;
using Shipment.Domain.Entities;
using Shipment.Infrastructure.Persistence.Fulfillment;

namespace Shipment.Infrastructure.Persistence;

public sealed class ShipmentDbContext : DomainEventOutboxDbContext, IUnitOfWork
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Shipment.Domain.Entities.Shipment> Shipments => Set<Shipment.Domain.Entities.Shipment>();
    public DbSet<ShippingCarrier> ShippingCarriers => Set<ShippingCarrier>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<ShipmentPackage> ShipmentPackages => Set<ShipmentPackage>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
    public DbSet<ShipmentFulfillmentSequence> ShipmentFulfillmentSequences => Set<ShipmentFulfillmentSequence>();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // Allocate revisions before EF writes, within the same transaction as Shipment and Outbox.
        var existingTransaction = Database.CurrentTransaction;
        await using var ownedTransaction = existingTransaction is null
            ? await Database.BeginTransactionAsync(cancellationToken)
            : null;
        var transaction = existingTransaction ?? ownedTransaction!;
        string savepoint = $"fulfillment_{Guid.NewGuid():N}";
        if (existingTransaction is not null)
            await transaction.CreateSavepointAsync(savepoint, cancellationToken);

        var existingOutboxIds = OutboxMessages.Local.Select(x => x.Id).ToHashSet();
        try
        {
            int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (ownedTransaction is not null)
                await ownedTransaction.CommitAsync(cancellationToken);
            else
                await transaction.ReleaseSavepointAsync(savepoint, cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            if (ownedTransaction is not null)
                await ownedTransaction.RollbackAsync(CancellationToken.None);
            else
                await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None);

            // A later attempt must allocate again because the counter increment was rolled back.
            foreach (var entry in ChangeTracker.Entries<OutboxMessage>()
                .Where(x => !existingOutboxIds.Contains(x.Entity.Id)).ToArray())
                entry.State = EntityState.Detached;
            if (exception is DbUpdateException
                { InnerException: Npgsql.PostgresException { SqlState: "23505", ConstraintName: "IX_Shipments_StoreId_OrderId" } })
            {
                var added = ChangeTracker.Entries<global::Shipment.Domain.Entities.Shipment>()
                    .FirstOrDefault(x => x.State == EntityState.Added);
                if (added is not null)
                    throw new global::Shipment.Application.Exceptions.ShipmentAlreadyExistsForOrderException(added.Entity.OrderId);
            }
            throw;
        }
    }

    protected override async Task StageEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent is ShipmentFulfillmentChangedDomainEvent fulfillment)
        {
            long revision = await ShipmentFulfillmentRevisionAllocator.NextAsync(
                this, fulfillment.StoreId, fulfillment.OrderId, cancellationToken);
            StageIntegrationEvent(fulfillment.StoreId, new ShipmentFulfillmentChangedIntegrationEvent(
                fulfillment.Id, fulfillment.OccurredOnUtc, fulfillment.StoreId, fulfillment.OrderId,
                fulfillment.ShipmentId, fulfillment.ShipmentNumber,
                fulfillment.Status switch
                {
                    global::Shipment.Domain.Enums.ShipmentStatus.Draft => ShipmentFulfillmentStatus.Draft,
                    global::Shipment.Domain.Enums.ShipmentStatus.ReadyForDispatch => ShipmentFulfillmentStatus.ReadyForDispatch,
                    global::Shipment.Domain.Enums.ShipmentStatus.Shipped => ShipmentFulfillmentStatus.Shipped,
                    global::Shipment.Domain.Enums.ShipmentStatus.Delivered => ShipmentFulfillmentStatus.Delivered,
                    global::Shipment.Domain.Enums.ShipmentStatus.DeliveryException => ShipmentFulfillmentStatus.DeliveryException,
                    global::Shipment.Domain.Enums.ShipmentStatus.Cancelled => ShipmentFulfillmentStatus.Cancelled,
                    _ => throw new InvalidOperationException("Unsupported shipment fulfillment status.")
                }, revision));
            return;
        }
        if (domainEvent is not ShipmentStatusChangedDomainEvent e)
            throw new InvalidOperationException($"Unsupported Shipment event {domainEvent.GetType().Name}.");
        var shipment = Shipments.Local.Single(x => x.Id == e.ShipmentId);
        StageIntegrationEvent(e.StoreId, new ShipmentStatusChangedIntegrationEvent(
            e.Id, e.OccurredOnUtc, e.ShipmentId, e.StoreId, e.OrderId, shipment.CustomerId,
            shipment.OrderNumber, shipment.ShipmentNumber, e.Kind switch
            {
                Shipment.Domain.Events.ShipmentEventKind.Created => Shipment.Contracts.IntegrationEvents.ShipmentEventKind.Created,
                Shipment.Domain.Events.ShipmentEventKind.Shipped => Shipment.Contracts.IntegrationEvents.ShipmentEventKind.Shipped,
                Shipment.Domain.Events.ShipmentEventKind.Delivered => Shipment.Contracts.IntegrationEvents.ShipmentEventKind.Delivered,
                Shipment.Domain.Events.ShipmentEventKind.DeliveryException => Shipment.Contracts.IntegrationEvents.ShipmentEventKind.DeliveryException,
                _ => throw new InvalidOperationException("Unsupported shipment event kind.")
            }, shipment.CustomerEmail, shipment.CustomerFullName, shipment.CarrierName,
            shipment.Packages.OrderBy(x => x.CreatedAtUtc).Select(x => x.TrackingNumber).FirstOrDefault(x => x != null),
            shipment.TrackingUrl, e.Description));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipmentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
