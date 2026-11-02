using System.Data.Common;
using System.Text.Json;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Consuming;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Order.Application.Events;
using Order.Domain.Enums;
using Order.Domain.Models;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Messaging.Shipment;
using Order.Infrastructure.Persistence;
using RabbitMQ.Client;
using Shipment.Application.Exceptions;
using Shipment.Contracts.IntegrationEvents;
using Shipment.Domain.ValueObjects;
using Shipment.Infrastructure.Persistence;
using OrderEntity = Order.Domain.Entities.Order;
using ShipmentEntity = Shipment.Domain.Entities.Shipment;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class ShipmentOrderFulfillmentTests
{
    [TestMethod]
    public async Task Source_SavesImmutableSnapshotsAndPerOrderRevisionsInOneCommit()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = ShipmentContext(connection);
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        var shipment = Shipment(Guid.NewGuid(), Guid.NewGuid(), "SHP-1");
        shipment.MarkReadyForDispatch();
        shipment.MarkShipped();
        shipment.MarkDelivered();
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();

        var events = await FulfillmentEvents(db);
        CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4 }, events.Select(x => x.FulfillmentRevision).ToArray());
        CollectionAssert.AreEqual(new[] { ShipmentFulfillmentStatus.Draft, ShipmentFulfillmentStatus.ReadyForDispatch,
            ShipmentFulfillmentStatus.Shipped, ShipmentFulfillmentStatus.Delivered }, events.Select(x => x.Status).ToArray());
        Assert.AreEqual(4L, (await db.ShipmentFulfillmentSequences.SingleAsync()).LastRevision);
        Assert.AreEqual(4, events.Select(x => x.Id).Distinct().Count());
        Assert.AreEqual(3, await db.OutboxMessages.CountAsync(x => x.EventType == ShipmentStatusChangedIntegrationEvent.EventType));
    }

    [TestMethod]
    public async Task Source_ExplicitRollbackRollsBackShipmentCounterAndOutboxTogether()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = ShipmentContext(connection);
        await db.Database.MigrateAsync();
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            db.Shipments.Add(Shipment(Guid.NewGuid(), Guid.NewGuid(), "ROLLBACK"));
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }
        await using var verify = ShipmentContext(connection);
        Assert.AreEqual(0, await verify.Shipments.CountAsync());
        Assert.AreEqual(0, await verify.ShipmentFulfillmentSequences.CountAsync());
        Assert.AreEqual(0, await verify.OutboxMessages.CountAsync());
    }

    [TestMethod]
    public async Task Source_FailedSaveReallocatesRevisionWithoutDuplicateOutbox()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = new ShipmentDbContext(new DbContextOptionsBuilder<ShipmentDbContext>()
            .UseNpgsql(connection).AddInterceptors(new FailFirstSave()).Options);
        await db.Database.MigrateAsync();
        var shipment = Shipment(Guid.NewGuid(), Guid.NewGuid(), "RETRY");
        db.Shipments.Add(shipment);
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.AreEqual(0, await db.ShipmentFulfillmentSequences.CountAsync());
        Assert.IsNotEmpty(shipment.DomainEvents);
        await db.SaveChangesAsync();
        Assert.AreEqual(1L, (await FulfillmentEvents(db)).Single().FulfillmentRevision);
        Assert.AreEqual(2, await db.OutboxMessages.CountAsync());
    }

    [TestMethod]
    public async Task Source_CancelAndReplacementContinueTheSameOrderCounter()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = ShipmentContext(connection);
        await db.Database.MigrateAsync();
        var original = Shipment(Guid.NewGuid(), Guid.NewGuid(), "A");
        db.Shipments.Add(original);
        await db.SaveChangesAsync();
        original.Cancel("Replacement required");
        await db.SaveChangesAsync();
        var replacement = Shipment(original.StoreId, original.OrderId, "B");
        db.Shipments.Add(replacement);
        await db.SaveChangesAsync();
        var events = await FulfillmentEvents(db);
        CollectionAssert.AreEqual(new long[] { 1, 2, 3 }, events.Select(x => x.FulfillmentRevision).ToArray());
        Assert.AreEqual(replacement.Id, events[2].ShipmentId);
        db.Shipments.Add(Shipment(original.StoreId, original.OrderId, "C"));
        await Assert.ThrowsExactlyAsync<ShipmentAlreadyExistsForOrderException>(() => db.SaveChangesAsync());
        Assert.AreEqual(3L, (await db.ShipmentFulfillmentSequences.AsNoTracking().SingleAsync()).LastRevision);
    }

    [TestMethod]
    public async Task Source_StaleWriterCannotOverwriteShipmentOrPublishAConflictingRevision()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var first = ShipmentContext(connection);
        await first.Database.MigrateAsync();
        var shipment = Shipment(Guid.NewGuid(), Guid.NewGuid(), "CONCURRENT");
        first.Shipments.Add(shipment);
        await first.SaveChangesAsync();
        await using var second = ShipmentContext(connection);
        var stale = await second.Shipments.SingleAsync();
        shipment.MarkShipped();
        await first.SaveChangesAsync();
        stale.Cancel("Stale cancellation");
        await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        Assert.AreEqual(2L, (await first.ShipmentFulfillmentSequences.AsNoTracking().SingleAsync()).LastRevision);
        Assert.HasCount(2, await FulfillmentEvents(first));
    }

    [TestMethod]
    public async Task Migration_ExistingShipmentGetsBaselineOutboxAndNextRevisionContinues()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = ShipmentContext(connection);
        await db.GetService<IMigrator>().MigrateAsync("20261028165032_AddShipmentCustomerContact");
        var shipment = Shipment(Guid.NewGuid(), Guid.NewGuid(), "EXISTING");
        shipment.ClearDomainEvents();
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        await db.Database.MigrateAsync();
        var baseline = (await FulfillmentEvents(db)).Single();
        Assert.AreEqual(shipment.Id, baseline.ShipmentId);
        Assert.AreEqual(1L, baseline.FulfillmentRevision);
        shipment.MarkShipped();
        await db.SaveChangesAsync();
        Assert.AreEqual(2L, (await FulfillmentEvents(db)).Last().FulfillmentRevision);
    }

    [TestMethod]
    public async Task Inbox_DuplicatesAndOlderSnapshotsAreProcessedWithoutRegressingOrder()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = Provider(connection);
        var order = await SeedOrder(services);
        var latest = Event(order, ShipmentFulfillmentStatus.Delivered, 4);
        await Receive(services, latest);
        await Receive(services, latest);
        await Processor(services).ProcessBatchAsync();
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Draft, 1, latest.ShipmentId));
        await Processor(services).ProcessBatchAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var saved = await db.Orders.SingleAsync();
        Assert.AreEqual(FulfillmentStatus.Delivered, saved.FulfillmentStatus);
        Assert.AreEqual(4L, saved.LastFulfillmentRevision);
        Assert.AreEqual(2, await db.InboxMessages.CountAsync(x => x.Status == InboxMessageStatus.Processed));
    }

    [TestMethod]
    public async Task Inbox_CommitFailureRollsBackOrderAndProcessedState_ThenRetries()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = Provider(connection, interceptor: new FailProcessedUpdate());
        var order = await SeedOrder(services);
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Shipped, 2));
        await Processor(services).ProcessBatchAsync();
        await using (var scope = services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            Assert.AreEqual(0L, (await db.Orders.SingleAsync()).LastFulfillmentRevision);
            Assert.AreEqual(InboxMessageStatus.Pending, (await db.InboxMessages.SingleAsync()).Status);
            await db.Database.ExecuteSqlRawAsync("UPDATE \"InboxMessages\" SET \"NextAttemptAtUtc\" = NOW()");
        }
        await Processor(services).ProcessBatchAsync();
        await using var verify = services.CreateAsyncScope();
        var saved = verify.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.AreEqual(2L, (await saved.Orders.SingleAsync()).LastFulfillmentRevision);
        Assert.AreEqual(InboxMessageStatus.Processed, (await saved.InboxMessages.SingleAsync()).Status);
    }

    [TestMethod]
    public async Task Inbox_InvalidPayloadFailsPermanently_AndWrongTenantCannotUpdateOrder()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = Provider(connection, maximumAttempts: 1);
        var order = await SeedOrder(services);
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Shipped, 2) with { StoreId = Guid.NewGuid() });
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Shipped, 3) with { Status = (ShipmentFulfillmentStatus)999 });
        await Processor(services).ProcessBatchAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.AreEqual(0L, (await db.Orders.SingleAsync()).LastFulfillmentRevision);
        Assert.AreEqual(2, await db.InboxMessages.CountAsync(x => x.Status == InboxMessageStatus.Failed));
    }

    [TestMethod]
    public async Task Inbox_ConcurrentClaimsAreDisjoint_ExpiredOwnerCannotComplete()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = Provider(connection);
        var order = await SeedOrder(services);
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Draft, 1));
        await Receive(services, Event(order, ShipmentFulfillmentStatus.Shipped, 2));
        await using var a = services.CreateAsyncScope();
        await using var b = services.CreateAsyncScope();
        var first = a.ServiceProvider.GetRequiredService<IModuleInboxStore<OrderDbContext>>();
        var second = b.ServiceProvider.GetRequiredService<IModuleInboxStore<OrderDbContext>>();
        var now = DateTime.UtcNow;
        var claims = await Task.WhenAll(first.ClaimAsync(Guid.NewGuid(), now, now.AddMinutes(1), 1),
            second.ClaimAsync(Guid.NewGuid(), now, now.AddMinutes(1), 1));
        Assert.AreNotEqual(claims[0].Single().Id, claims[1].Single().Id);
        var recovered = await second.ClaimAsync(Guid.NewGuid(), now.AddMinutes(2), now.AddMinutes(3), 2);
        Assert.HasCount(2, recovered);
        Assert.IsFalse(await first.MarkProcessedAsync(claims[0][0].Id, claims[0][0].LockId, now.AddMinutes(2)));
    }

    [TestMethod]
    public async Task Order_ConcurrentDifferentMessagesUseRowVersionToPreventLostUpdates()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = Provider(connection);
        var order = await SeedOrder(services);
        await using var a = services.CreateAsyncScope();
        await using var b = services.CreateAsyncScope();
        var first = a.ServiceProvider.GetRequiredService<OrderDbContext>();
        var second = b.ServiceProvider.GetRequiredService<OrderDbContext>();
        var older = await first.Orders.SingleAsync();
        var newer = await second.Orders.SingleAsync();
        var shipment = Guid.NewGuid();
        older.ApplyShipmentFulfillment(shipment, "SHP", ShipmentFulfillmentState.Shipped, 2);
        newer.ApplyShipmentFulfillment(shipment, "SHP", ShipmentFulfillmentState.Delivered, 3);
        await second.SaveChangesAsync();
        await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() => first.SaveChangesAsync());
        await first.Entry(older).ReloadAsync();
        Assert.IsFalse(older.ApplyShipmentFulfillment(shipment, "SHP", ShipmentFulfillmentState.Shipped, 2));
        Assert.AreEqual(3L, older.LastFulfillmentRevision);
    }

    [TestMethod]
    public async Task RabbitMq_SourceOutboxToOrderInboxAndProjection_IsDurableAndIdempotent()
    {
        string sourceConnection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        string orderConnection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        string exchange = "fulfillment." + Guid.NewGuid().ToString("N");
        var rabbit = RabbitMqTestContainer.CreateOptions(exchange);
        var ingressFailure = new FailInboxInsert();
        await using var services = Provider(orderConnection, rabbit, ingressFailure);
        var order = await SeedOrder(services);
        var options = services.GetRequiredService<IOptions<RabbitMqConsumerOptions<OrderFulfillmentConsumerIdentity>>>();
        await using var broker = await RabbitMqTestContainer.CreateConnectionAsync();
        await using var channel = await broker.CreateChannelAsync();
        await RabbitMqConsumerTopology.DeclareAsync(channel, exchange, options.Value);
        var consumer = services.GetRequiredService<RabbitMqIntegrationEventConsumer<OrderFulfillmentConsumerIdentity>>();
        await consumer.StartAsync(CancellationToken.None);
        try
        {
            await using var source = ShipmentContext(sourceConnection);
            await source.Database.MigrateAsync();
            var shipment = Shipment(order.StoreId, order.Id, "SHP-E2E");
            shipment.MarkShipped();
            shipment.MarkDelivered();
            source.Shipments.Add(shipment);
            await source.SaveChangesAsync();
            // Existing notifications have their own subscriber; bind an observer to preserve mandatory publishing.
            string observer = exchange + ".notifications";
            await channel.QueueDeclareAsync(observer, true, false, false);
            await channel.QueueBindAsync(observer, exchange, ShipmentStatusChangedIntegrationEvent.EventType);
            string fulfillmentObserver = exchange + ".fulfillment-observer";
            await channel.QueueDeclareAsync(fulfillmentObserver, true, false, false);
            await channel.QueueBindAsync(fulfillmentObserver, exchange, ShipmentFulfillmentChangedIntegrationEvent.EventType);
            var publisher = services.GetRequiredService<IIntegrationEventPublisher<OrderFulfillmentConsumerIdentity>>();
            var processor = new ModuleOutboxProcessor<ShipmentDbContext, OrderFulfillmentConsumerIdentity>(
                new ModuleOutboxStore<ShipmentDbContext>(source), publisher,
                Microsoft.Extensions.Options.Options.Create(new ModuleOutboxOptions<OrderFulfillmentConsumerIdentity>()), TimeProvider.System,
                NullLogger<ModuleOutboxProcessor<ShipmentDbContext, OrderFulfillmentConsumerIdentity>>.Instance);
            await processor.ProcessBatchAsync();
            await WaitUntil(async () =>
            {
                await Processor(services).ProcessBatchAsync();
                await using var scope = services.CreateAsyncScope();
                return await scope.ServiceProvider.GetRequiredService<OrderDbContext>().Orders
                    .AnyAsync(x => x.Id == order.Id && x.LastFulfillmentRevision == 3);
            });
            var duplicate = (await source.OutboxMessages.AsNoTracking()
                .Where(x => x.EventType == ShipmentFulfillmentChangedIntegrationEvent.EventType).ToArrayAsync()).First();
            await publisher.PublishAsync(new(duplicate.Id, duplicate.EventType, duplicate.RoutingKey,
                duplicate.Payload, duplicate.OccurredOnUtc));
            await WaitUntil(async () => (await channel.QueueDeclarePassiveAsync(options.Value.QueueName)).MessageCount == 0);
            await using var verify = services.CreateAsyncScope();
            var db = verify.ServiceProvider.GetRequiredService<OrderDbContext>();
            Assert.AreEqual(3, await db.InboxMessages.CountAsync());
            Assert.AreEqual(FulfillmentStatus.Delivered, (await db.Orders.SingleAsync()).FulfillmentStatus);
            Assert.IsTrue(await source.OutboxMessages.AllAsync(x => x.Status == OutboxMessageStatus.Published));
            Assert.IsTrue(ingressFailure.Failed);
            Assert.AreEqual(4u, (await channel.QueueDeclarePassiveAsync(fulfillmentObserver)).MessageCount,
                "Three original events and one deliberate duplicate; ingress retry must not fan out to other subscribers.");

            var invalidId = Guid.NewGuid();
            await publisher.PublishAsync(new(invalidId, ShipmentFulfillmentChangedIntegrationEvent.EventType,
                ShipmentFulfillmentChangedIntegrationEvent.EventType, "{", DateTime.UtcNow));
            BasicGetResult? deadLetter = null;
            await WaitUntil(async () => (deadLetter = await channel.BasicGetAsync(
                RabbitMqConsumerTopology.DeadLetterQueue(options.Value.QueueName), true)) is not null);
            Assert.AreEqual(invalidId.ToString("D"), deadLetter!.BasicProperties.MessageId);
            Assert.AreEqual(3, await db.InboxMessages.CountAsync());
        }
        finally { await consumer.StopAsync(CancellationToken.None); }
    }

    private static ServiceProvider Provider(string connection, RabbitMqOptions? rabbit = null,
        IInterceptor? interceptor = null, int maximumAttempts = 10)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseNpgsql(connection);
            if (interceptor is not null) options.AddInterceptors(interceptor);
        });
        services.AddScoped<IOrderDomainEventDispatcher, NoEventsDispatcher>();
        services.AddScoped<IModuleInboxStore<OrderDbContext>, ModuleInboxStore<OrderDbContext>>();
        services.AddScoped<IIntegrationMessageReceiver<OrderFulfillmentConsumerIdentity>,
            ModuleInboxReceiver<OrderDbContext, OrderFulfillmentConsumerIdentity>>();
        services.AddScoped<IModuleInboxMessageHandler<OrderFulfillmentConsumerIdentity>, ShipmentFulfillmentInboxHandler>();
        services.AddSingleton<IOptions<ModuleInboxOptions<OrderFulfillmentConsumerIdentity>>>(Microsoft.Extensions.Options.Options.Create(
            new ModuleInboxOptions<OrderFulfillmentConsumerIdentity> { MaximumAttempts = maximumAttempts,
                InitialRetryDelay = TimeSpan.FromMilliseconds(1), RetryJitterFactor = 0 }));
        services.AddSingleton<ModuleInboxProcessor<OrderDbContext, OrderFulfillmentConsumerIdentity>>();
        if (rabbit is not null)
        {
            services.AddSingleton<IOptions<RabbitMqOptions>>(Microsoft.Extensions.Options.Options.Create(rabbit));
            services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
            services.AddSingleton<IIntegrationEventPublisher<OrderFulfillmentConsumerIdentity>, RabbitMqIntegrationEventPublisher<OrderFulfillmentConsumerIdentity>>();
            services.AddSingleton<IRabbitMqFailureMessagePublisher<OrderFulfillmentConsumerIdentity>, RabbitMqFailureMessagePublisher<OrderFulfillmentConsumerIdentity>>();
            services.AddSingleton<IOptions<RabbitMqConsumerOptions<OrderFulfillmentConsumerIdentity>>>(Microsoft.Extensions.Options.Options.Create(
                new RabbitMqConsumerOptions<OrderFulfillmentConsumerIdentity> { Enabled = true,
                    QueueName = rabbit.ExchangeName + ".order", RoutingKeys = [ShipmentFulfillmentChangedIntegrationEvent.EventType],
                    Retry = new RabbitMqRetryOptions { Delays = [TimeSpan.FromMilliseconds(100)] } }));
            services.AddSingleton<RabbitMqDeliveryFailureRouter<OrderFulfillmentConsumerIdentity>>();
            services.AddSingleton<RabbitMqIntegrationEventConsumer<OrderFulfillmentConsumerIdentity>>();
        }
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static ModuleInboxProcessor<OrderDbContext, OrderFulfillmentConsumerIdentity> Processor(IServiceProvider services) =>
        services.GetRequiredService<ModuleInboxProcessor<OrderDbContext, OrderFulfillmentConsumerIdentity>>();

    private static async Task<OrderEntity> SeedOrder(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        var order = OrderEntity.Place(Guid.NewGuid(), OrderNumber.Create("ORD-" + Guid.NewGuid().ToString("N")[..10]),
            CustomerSnapshot.Create(Guid.NewGuid(), "customer@example.com", "Customer", "+905550000000"),
            Address(), Address(), "TRY", [new OrderItemDraft(Guid.NewGuid(), null, "Product", null, "SKU", 1,
                OrderPriceSnapshot.Create(100m, "TRY", null, Guid.NewGuid(), Guid.NewGuid()))]);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    private static ShipmentFulfillmentChangedIntegrationEvent Event(OrderEntity order, ShipmentFulfillmentStatus status,
        long revision, Guid? shipmentId = null) =>
        new(Guid.NewGuid(), DateTime.UtcNow, order.StoreId, order.Id, shipmentId ?? Guid.NewGuid(), "SHP", status, revision);
    private static async Task Receive(IServiceProvider services, ShipmentFulfillmentChangedIntegrationEvent message)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIntegrationMessageReceiver<OrderFulfillmentConsumerIdentity>>()
            .ReceiveAsync(new(message.Id, ShipmentFulfillmentChangedIntegrationEvent.EventType,
                ShipmentFulfillmentChangedIntegrationEvent.EventType, JsonSerializer.Serialize(message, JsonSerializerOptions.Web),
                message.OccurredOnUtc));
    }
    private static async Task<ShipmentFulfillmentChangedIntegrationEvent[]> FulfillmentEvents(ShipmentDbContext db) =>
        (await db.OutboxMessages.AsNoTracking().Where(x => x.EventType == ShipmentFulfillmentChangedIntegrationEvent.EventType)
            .ToArrayAsync()).Select(x => JsonSerializer.Deserialize<ShipmentFulfillmentChangedIntegrationEvent>(
                x.Payload, JsonSerializerOptions.Web)!).OrderBy(x => x.FulfillmentRevision).ToArray();
    private static ShipmentDbContext ShipmentContext(string connection) => new(
        new DbContextOptionsBuilder<ShipmentDbContext>().UseNpgsql(connection).Options);
    private static ShipmentEntity Shipment(Guid store, Guid order, string number)
    {
        var shipment = ShipmentEntity.Create(store, order,
            "ORD-1", number, "Customer", "+905550000000",
            ShipmentAddress.Create("Customer", "+905550000000", "Turkey", "Istanbul", "Kadikoy", "Street", null, "34000"),
            [new global::Shipment.Domain.Entities.ShipmentLineDraft(Guid.NewGuid(), Guid.NewGuid(), null, "Product", null, "SKU", 1)]);
        shipment.AddPackage("PKG-1", "TRACK-1", null, null, null);
        return shipment;
    }
    private static OrderAddressSnapshot Address() => OrderAddressSnapshot.Create("Home", "Customer", "+905550000000",
        "Turkey", "Istanbul", "Kadikoy", "Street", null, "34000");
    private static async Task WaitUntil(Func<Task<bool>> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        while (!await condition()) await Task.Delay(50, timeout.Token);
    }
    private sealed class NoEventsDispatcher : IOrderDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            if (events.Count != 0) throw new InvalidOperationException("Unexpected Order event in fulfillment test.");
            return Task.CompletedTask;
        }
    }
    private sealed class FailFirstSave : SaveChangesInterceptor
    {
        private bool _failed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_failed) { _failed = true; throw new DbUpdateException("Simulated source persistence failure."); }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
    private sealed class FailProcessedUpdate : DbCommandInterceptor
    {
        private bool _failed;
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_failed && command.CommandText.Contains("SET \"Status\" = 2", StringComparison.Ordinal))
            { _failed = true; throw new IOException("Simulated Inbox completion failure after Order SaveChanges."); }
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
    private sealed class FailInboxInsert : DbCommandInterceptor
    {
        public bool Failed { get; private set; }
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!Failed && command.CommandText.Contains("INSERT INTO \"InboxMessages\"", StringComparison.Ordinal))
            { Failed = true; throw new IOException("Simulated database outage before Inbox persistence."); }
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
