using System.Data.Common;
using System.Text.Json;
using BuildingBlocks.Infrastructure.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Consuming;
using BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.DependencyInjection;
using Inventory.Infrastructure.Messaging.Order;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Order.Application.Abstractions;
using Order.Application.Events;
using Order.Contracts;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Enums;
using Order.Domain.Models;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Integrations.Payment;
using Order.Infrastructure.Options;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.ReadServices;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RabbitMQ.Client;
using OrderEntity = Order.Domain.Entities.Order;
using PaymentState = Payment.Domain.Enums.PaymentStatus;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class OrderInventoryCancellationTests
{
    [TestMethod]
    public async Task Cancellation_PersistsNotificationAndReservationFactsAtomically()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = OrderProvider(connection);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
        var order = Order();
        order.SetReservationReference("RES-1");
        order.Cancel("Customer changed mind");
        order.Cancel("Duplicate cancellation");
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var messages = await db.OutboxMessages.AsNoTracking().ToArrayAsync();
        Assert.HasCount(2, messages);
        var reservation = messages.Single(x => x.EventType == OrderReservationCancelledIntegrationEvent.EventType);
        var notification = messages.Single(x => x.EventType == OrderCancelledIntegrationEvent.EventType);
        var payload = JsonSerializer.Deserialize<OrderReservationCancelledIntegrationEvent>(reservation.Payload, JsonSerializerOptions.Web)!;
        Assert.AreNotEqual(notification.Id, reservation.Id);
        Assert.AreEqual(order.Id, payload.OrderId);
        Assert.AreEqual(order.StoreId, payload.StoreId);
        Assert.AreEqual("RES-1", payload.ReservationReference);
        Assert.AreEqual(reservation.Id, payload.Id);
        Assert.IsFalse(reservation.Payload.Contains("customer@example.com", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Cancellation_RollbackLeavesOrderAndBothOutboxFactsUncommitted()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = OrderProvider(connection);
        var order = Order();
        order.SetReservationReference("RES-ROLLBACK");
        await using (var scope = services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.MigrateAsync();
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            order.Cancel("Rollback");
            await db.SaveChangesAsync();
            await tx.RollbackAsync();
        }
        await using var verify = services.CreateAsyncScope();
        var saved = verify.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.AreEqual(OrderStatus.Confirmed, (await saved.Orders.SingleAsync()).Status);
        Assert.AreEqual(0, await saved.OutboxMessages.CountAsync());
    }

    [TestMethod]
    public async Task Inbox_MultipleItemsReleaseAtomically_AndBusinessDuplicatesDoNotRepeatMovements()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection);
        var cancellation = await SeedReservation(services);
        await Receive(services, cancellation);
        await Receive(services, cancellation);
        await Processor(services).ProcessBatchAsync();
        await Receive(services, cancellation with { Id = Guid.NewGuid() });
        await Processor(services).ProcessBatchAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var items = await db.InventoryItems.ToArrayAsync();
        Assert.IsTrue(items.All(x => x.ReservedQuantity == 0 && x.OnHandQuantity == 10));
        Assert.AreEqual(2, await db.StockMovements.CountAsync(x => x.Type == StockMovementType.ReservationReleased));
        Assert.AreEqual(2, await db.InboxMessages.CountAsync(x => x.Status == InboxMessageStatus.Processed));
        Assert.AreEqual(2, await db.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Released));
    }

    [TestMethod]
    public async Task Inbox_ConfirmedReservationFailsWithoutReleasingOtherItems()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection);
        var cancellation = await SeedReservation(services);
        await using (var scope = services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var item = await db.InventoryItems.Include(x => x.Reservations).FirstAsync();
            item.ConfirmReservation(cancellation.ReservationReference, "Payment captured");
            await db.SaveChangesAsync();
        }
        await Receive(services, cancellation);
        await Processor(services).ProcessBatchAsync();
        await using var verify = services.CreateAsyncScope();
        var saved = verify.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Assert.AreEqual(InboxMessageStatus.Failed, (await saved.InboxMessages.SingleAsync()).Status);
        Assert.AreEqual(0, await saved.StockMovements.CountAsync(x => x.Type == StockMovementType.ReservationReleased));
        Assert.AreEqual(1, await saved.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Active));
        Assert.AreEqual(1, await saved.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Confirmed));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Inbox_WrongTenantOrOrderCannotReleaseReservation(bool wrongTenant)
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection, maximumAttempts: 1);
        var original = await SeedReservation(services);
        var cancellation = wrongTenant
            ? original with { StoreId = Guid.NewGuid() }
            : original with { OrderId = Guid.NewGuid() };
        await Receive(services, cancellation);
        await Processor(services).ProcessBatchAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Assert.AreEqual(2, await db.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Active));
        Assert.AreEqual(InboxMessageStatus.Failed, (await db.InboxMessages.SingleAsync()).Status);
    }

    [TestMethod]
    public async Task Inbox_CompletionFailureRollsBackStockAndMovements_ThenRetries()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection, interceptor: new FailCompletion());
        var cancellation = await SeedReservation(services);
        await Receive(services, cancellation);
        await Processor(services).ProcessBatchAsync();
        await using (var scope = services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            Assert.AreEqual(2, await db.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Active));
            Assert.AreEqual(0, await db.StockMovements.CountAsync(x => x.Type == StockMovementType.ReservationReleased));
            Assert.AreEqual(InboxMessageStatus.Pending, (await db.InboxMessages.SingleAsync()).Status);
            await db.Database.ExecuteSqlRawAsync("UPDATE \"InboxMessages\" SET \"NextAttemptAtUtc\" = NOW()");
        }
        await Processor(services).ProcessBatchAsync();
        await using var verify = services.CreateAsyncScope();
        Assert.AreEqual(2, await verify.ServiceProvider.GetRequiredService<InventoryDbContext>().InventoryReservations
            .CountAsync(x => x.Status == InventoryReservationStatus.Released));
    }

    [TestMethod]
    public async Task Inbox_ConcurrentWorkersDoNotDoubleReleaseStock()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection);
        var cancellation = await SeedReservation(services);
        await Receive(services, cancellation);
        await Receive(services, cancellation with { Id = Guid.NewGuid() });
        await Task.WhenAll(Processor(services).ProcessBatchAsync(), Processor(services).ProcessBatchAsync());
        await using (var scope = services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.ExecuteSqlRawAsync(
                "UPDATE \"InboxMessages\" SET \"NextAttemptAtUtc\" = NOW() WHERE \"Status\" = 0");
        await Processor(services).ProcessBatchAsync();
        await using var verify = services.CreateAsyncScope();
        var db = verify.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Assert.AreEqual(2, await db.StockMovements.CountAsync(x => x.Type == StockMovementType.ReservationReleased));
        Assert.IsTrue(await db.InventoryItems.AllAsync(x => x.ReservedQuantity == 0 && x.OnHandQuantity == 10));
        Assert.AreEqual(2, await db.InboxMessages.CountAsync(x => x.Status == InboxMessageStatus.Processed));
    }

    [TestMethod]
    public async Task Inventory_ConcurrentConfirmationAndReleaseCannotBothCommit()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var services = InventoryProvider(connection);
        var cancellation = await SeedReservation(services);
        await using var a = services.CreateAsyncScope();
        await using var b = services.CreateAsyncScope();
        var first = a.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var second = b.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var confirm = await first.InventoryItems.Include(x => x.Reservations).FirstAsync();
        var release = await second.InventoryItems.Include(x => x.Reservations).SingleAsync(x => x.Id == confirm.Id);
        confirm.ConfirmReservation(cancellation.ReservationReference, "Captured");
        release.ReleaseReservation(cancellation.ReservationReference, "Cancelled");
        await first.SaveChangesAsync();
        await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await second.Entry(release).ReloadAsync();
        Assert.AreEqual(confirm.OnHandQuantity, release.OnHandQuantity);
    }

    [TestMethod]
    public async Task OperationLock_IsSharedAcrossInstances_AndReleasedAfterDispose()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        var settings = Microsoft.Extensions.Options.Options.Create(new OrderDatabaseOptions { ConnectionString = connection });
        var first = new PostgreSqlOrderOperationLock(settings);
        var second = new PostgreSqlOrderOperationLock(settings);
        var store = Guid.NewGuid();
        var order = Guid.NewGuid();
        await using (var lease = await first.AcquireAsync(store, order))
        {
            await Assert.ThrowsExactlyAsync<OrderOperationInProgressException>(() => second.AcquireAsync(store, order));
            await using var unrelated = await second.AcquireAsync(Guid.NewGuid(), order);
        }
        await using var recovered = await second.AcquireAsync(store, order);
    }

    [TestMethod]
    [DataRow(PaymentState.Pending, false)]
    [DataRow(PaymentState.RequiresAction, false)]
    [DataRow(PaymentState.Authorized, false)]
    [DataRow(PaymentState.Captured, false)]
    [DataRow(PaymentState.Refunded, false)]
    [DataRow(PaymentState.PartiallyRefunded, false)]
    [DataRow(PaymentState.Cancelled, true)]
    [DataRow(PaymentState.Failed, true)]
    public async Task PaymentGuard_UsesAuthoritativePaymentState(PaymentState state, bool allowed)
    {
        var order = Guid.NewGuid();
        var api = new PaymentReader { Result = new(Guid.NewGuid(), order, state, 100m, "TRY", null, null) };
        var guard = new OrderCancellationPaymentGuard(api);
        if (allowed) await guard.EnsureCanCancelAsync(Guid.NewGuid(), order);
        else await Assert.ThrowsExactlyAsync<global::Order.Application.Exceptions.OrderCancellationNotAllowedException>(
            () => guard.EnsureCanCancelAsync(Guid.NewGuid(), order));
    }

    [TestMethod]
    public async Task ProviderFence_PersistsUncertainAttemptBeforeAnOutcomeExists()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        var settings = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(connection).Options;
        await using var db = new PaymentDbContext(settings);
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        var payment = global::Payment.Domain.Entities.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1",
            Guid.NewGuid(), 100m, "TRY", global::Payment.Domain.Enums.PaymentProvider.Mock, global::Payment.Domain.Enums.PaymentMethodType.Card);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        Assert.IsFalse(payment.HasProviderAttempt);
        Assert.IsTrue(payment.RecordProviderAttemptStarted());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var snapshot = await new PaymentReadService(db).GetByOrderIdAsync(payment.StoreId, payment.OrderId);
        Assert.IsTrue(snapshot!.HasProviderAttempt);
        Assert.AreEqual(PaymentState.Pending, snapshot.Status);
        Assert.AreEqual(0, await db.OutboxMessages.CountAsync());
    }

    [TestMethod]
    public async Task Migration_LegacyPendingPaymentsAreTreatedAsUncertain()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(connection).Options);
        await db.Database.MigrateAsync();
        db.Payments.Add(global::Payment.Domain.Entities.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-LEGACY",
            Guid.NewGuid(), 100m, "TRY", global::Payment.Domain.Enums.PaymentProvider.Mock, global::Payment.Domain.Enums.PaymentMethodType.Card));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await db.GetService<IMigrator>().MigrateAsync("20261028165031_AddCustomerContactAndRefundIdempotency");
        await db.Database.MigrateAsync();
        Assert.IsTrue((await db.Payments.SingleAsync()).HasProviderAttempt);
    }

    [TestMethod]
    public async Task PaymentGuard_UntouchedPendingPaymentCanBeCancelled()
    {
        var order = Guid.NewGuid();
        var guard = new OrderCancellationPaymentGuard(new PaymentReader
        {
            Result = new(Guid.NewGuid(), order, PaymentState.Pending, 100m, "TRY", null, null, false)
        });
        await guard.EnsureCanCancelAsync(Guid.NewGuid(), order);
    }

    [TestMethod]
    public async Task PaymentGuard_UnavailablePaymentServiceDoesNotAllowCancellation()
    {
        var guard = new OrderCancellationPaymentGuard(new PaymentReader { Failure = new IOException("Database unavailable") });
        await Assert.ThrowsExactlyAsync<IOException>(() => guard.EnsureCanCancelAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [TestMethod]
    public async Task RabbitMq_OrderOutboxToInventoryInbox_ReleasesStockAfterConsumerStops()
    {
        string sourceConnection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        string inventoryConnection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        string exchange = "reservation." + Guid.NewGuid().ToString("N");
        await using var inventory = InventoryProvider(inventoryConnection, exchange);
        var order = Order();
        var cancellation = await SeedReservation(inventory, order.StoreId, order.Id);
        await using var source = OrderProvider(sourceConnection);
        await using var sourceScope = source.CreateAsyncScope();
        var db = sourceScope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
        order.SetReservationReference(cancellation.ReservationReference);
        order.Cancel("Customer cancellation");
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var options = inventory.GetRequiredService<IOptions<RabbitMqConsumerOptions<InventoryReservationConsumerIdentity>>>().Value;
        await using var broker = await RabbitMqTestContainer.CreateConnectionAsync();
        await using var channel = await broker.CreateChannelAsync();
        await RabbitMqConsumerTopology.DeclareAsync(channel, exchange, options);
        string notifications = exchange + ".notification-observer";
        await channel.QueueDeclareAsync(notifications, true, false, false);
        await channel.QueueBindAsync(notifications, exchange, OrderCancelledIntegrationEvent.EventType);
        var consumer = inventory.GetServices<Microsoft.Extensions.Hosting.IHostedService>()
            .OfType<RabbitMqIntegrationEventConsumer<InventoryReservationConsumerIdentity>>().Single();
        await consumer.StartAsync(CancellationToken.None);
        try
        {
            var publisher = inventory.GetRequiredService<IIntegrationEventPublisher<OrderOutboxPublisherIdentity>>();
            var outbox = new ModuleOutboxProcessor<OrderDbContext, OrderOutboxPublisherIdentity>(
                new ModuleOutboxStore<OrderDbContext>(db), publisher,
                Microsoft.Extensions.Options.Options.Create(new ModuleOutboxOptions<OrderOutboxPublisherIdentity>()),
                TimeProvider.System, NullLogger<ModuleOutboxProcessor<OrderDbContext, OrderOutboxPublisherIdentity>>.Instance);
            await outbox.ProcessBatchAsync();
            await WaitUntil(async () =>
            {
                await using var scope = inventory.CreateAsyncScope();
                return await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().InboxMessages.CountAsync() == 1;
            });
            Assert.AreEqual(1u, (await channel.QueueDeclarePassiveAsync(notifications)).MessageCount);
        }
        finally { await consumer.StopAsync(CancellationToken.None); }
        // Broker delivery is complete; local business processing remains recoverable after the receiver stops.
        await Processor(inventory).ProcessBatchAsync();
        await using var verify = inventory.CreateAsyncScope();
        var saved = verify.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Assert.AreEqual(InboxMessageStatus.Processed, (await saved.InboxMessages.SingleAsync()).Status);
        Assert.AreEqual(2, await saved.InventoryReservations.CountAsync(x => x.Status == InventoryReservationStatus.Released));
        Assert.AreEqual(2, await db.OutboxMessages.CountAsync(x => x.Status == global::Order.Infrastructure.Persistence.Outbox.OutboxMessageStatus.Published));
    }

    private static ServiceProvider InventoryProvider(string connection, string? exchange = null,
        IInterceptor? interceptor = null, int maximumAttempts = 10)
    {
        var rabbit = RabbitMqTestContainer.CreateOptions(exchange ?? "inventory." + Guid.NewGuid().ToString("N"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:Inventory:Database:ConnectionString"] = connection,
            ["Modules:Inventory:IntegrationEventConsumer:QueueName"] = rabbit.ExchangeName + ".inventory",
            ["Modules:Inventory:IntegrationEventConsumer:Enabled"] = "true",
            ["Modules:Inventory:IntegrationEventConsumer:Retry:Delays:0"] = "00:00:00.100",
            ["Modules:Inventory:InboxProcessor:MaximumAttempts"] = maximumAttempts.ToString(),
            ["Modules:Inventory:InboxProcessor:BatchSize"] = "1"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRabbitMqMessaging(configuration);
        services.Configure<BuildingBlocks.Messaging.RabbitMQ.Options.RabbitMqOptions>(x =>
        {
            x.HostName = rabbit.HostName; x.Port = rabbit.Port; x.UserName = rabbit.UserName;
            x.Password = rabbit.Password; x.ExchangeName = rabbit.ExchangeName;
            x.PublishingClientProvidedName = rabbit.PublishingClientProvidedName;
            x.ConsumingClientProvidedName = rabbit.ConsumingClientProvidedName;
        });
        services.AddInventoryInfrastructure(configuration);
        if (interceptor is not null)
            services.AddDbContext<InventoryDbContext>(x => x.AddInterceptors(interceptor));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static ServiceProvider OrderProvider(string connection)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(global::Order.Application.AssemblyReference).Assembly));
        services.AddDbContext<OrderDbContext>(x => x.UseNpgsql(connection));
        services.AddScoped<IOrderDomainEventDispatcher, OrderDomainEventDispatcher>();
        services.AddScoped<IOrderOutboxWriter, OrderOutboxWriter>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static ModuleInboxProcessor<InventoryDbContext, InventoryReservationConsumerIdentity> Processor(IServiceProvider services)
        => services.GetRequiredService<ModuleInboxProcessor<InventoryDbContext, InventoryReservationConsumerIdentity>>();
    private static async Task<OrderReservationCancelledIntegrationEvent> SeedReservation(IServiceProvider services,
        Guid? storeId = null, Guid? orderId = null)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        var store = storeId ?? Guid.NewGuid();
        var order = orderId ?? Guid.NewGuid();
        string reference = "RES-" + Guid.NewGuid().ToString("N");
        for (int i = 0; i < 2; i++)
        {
            var item = InventoryItem.Create(store, Guid.NewGuid(), null, "SKU-" + i, "Product", 10, null);
            item.Reserve(order, reference, i + 2);
            db.InventoryItems.Add(item);
        }
        await db.SaveChangesAsync();
        return new(Guid.NewGuid(), DateTime.UtcNow, store, order, reference, "Customer changed mind");
    }

    private static async Task Receive(IServiceProvider services, OrderReservationCancelledIntegrationEvent e)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIntegrationMessageReceiver<InventoryReservationConsumerIdentity>>()
            .ReceiveAsync(new(e.Id, OrderReservationCancelledIntegrationEvent.EventType,
                OrderReservationCancelledIntegrationEvent.EventType, JsonSerializer.Serialize(e, JsonSerializerOptions.Web), e.OccurredOnUtc));
    }

    private static OrderEntity Order()
    {
        var order = OrderEntity.Place(Guid.NewGuid(), OrderNumber.Create("ORD-" + Guid.NewGuid().ToString("N")),
            CustomerSnapshot.Create(Guid.NewGuid(), "customer@example.com", "Customer", "+905550000000"), Address(), Address(), "TRY",
            [new OrderItemDraft(Guid.NewGuid(), null, "Product", null, "SKU", 1,
                OrderPriceSnapshot.Create(100m, "TRY", null, Guid.NewGuid(), Guid.NewGuid()))]);
        return order;
    }
    private static OrderAddressSnapshot Address() => OrderAddressSnapshot.Create("Home", "Customer", "+905550000000",
        "Turkey", "Istanbul", "Kadikoy", "Street", null, "34000");
    private static async Task WaitUntil(Func<Task<bool>> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        while (!await condition()) await Task.Delay(50, timeout.Token);
    }
    private sealed class FailCompletion : DbCommandInterceptor
    {
        private bool _failed;
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_failed && command.CommandText.Contains("SET \"Status\" = 2", StringComparison.Ordinal))
            { _failed = true; throw new IOException("Simulated failure after Inventory SaveChanges."); }
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
    private sealed class PaymentReader : IPaymentModuleApi
    {
        public PaymentStatusResult? Result { get; set; }
        public Exception? Failure { get; set; }
        public Task<PaymentStatusResult?> GetByOrderIdAsync(GetPaymentByOrderIdRequest request, CancellationToken cancellationToken = default)
            => Failure is null ? Task.FromResult(Result) : Task.FromException<PaymentStatusResult?>(Failure);
        public Task<Guid> CreateForOrderAsync(CreatePaymentForOrderRequest r, CancellationToken c = default) => throw new NotSupportedException();
        public Task<PaymentOperationResult> AuthorizeAsync(AuthorizePaymentRequest r, CancellationToken c = default) => throw new NotSupportedException();
        public Task<PaymentOperationResult> CaptureAsync(CapturePaymentRequest r, CancellationToken c = default) => throw new NotSupportedException();
        public Task CancelAsync(CancelPaymentRequest r, CancellationToken c = default) => throw new NotSupportedException();
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest r, CancellationToken c = default) => throw new NotSupportedException();
    }
}
