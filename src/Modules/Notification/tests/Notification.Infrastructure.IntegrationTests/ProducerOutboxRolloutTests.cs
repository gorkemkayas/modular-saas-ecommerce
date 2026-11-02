using System.Text.Json;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Payment.Contracts.IntegrationEvents;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;
using Shipment.Contracts.IntegrationEvents;
using Shipment.Domain.ValueObjects;
using Shipment.Infrastructure.Persistence;
using PaymentEntity = Payment.Domain.Entities.Payment;
using ShipmentEntity = Shipment.Domain.Entities.Shipment;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class ProducerOutboxRolloutTests
{
    [TestMethod]
    public async Task SharedProcessor_PublishesPersistedEventAndFinalizesOwnership()
    {
        string connection = await NewDatabaseAsync();
        await using var db = PaymentContext(connection);
        await db.Database.MigrateAsync();
        var payment = CreatePayment();
        payment.MarkCaptured("capture", null, "reference");
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var publisher = new TestPublisher(false);
        var processor = new ModuleOutboxProcessor<PaymentDbContext, TestPublisher>(
            new ModuleOutboxStore<PaymentDbContext>(db), publisher,
            Microsoft.Extensions.Options.Options.Create(new ModuleOutboxOptions<TestPublisher>()),
            TimeProvider.System, NullLogger<ModuleOutboxProcessor<PaymentDbContext, TestPublisher>>.Instance);

        await processor.ProcessBatchAsync();

        var row = await db.OutboxMessages.AsNoTracking().SingleAsync();
        Assert.AreEqual(OutboxMessageStatus.Published, row.Status);
        Assert.IsNull(row.LockId);
        Assert.HasCount(1, publisher.Messages);
        Assert.AreEqual(row.Id, publisher.Messages[0].Id);
        Assert.AreEqual(PaymentStatusChangedIntegrationEvent.EventType, publisher.Messages[0].RoutingKey);
    }

    [TestMethod]
    public async Task SharedProcessor_ProviderFailureRetriesThenStopsAtAttemptLimit()
    {
        string connection = await NewDatabaseAsync();
        await using var db = PaymentContext(connection);
        await db.Database.MigrateAsync();
        var now = DateTime.UtcNow;
        db.OutboxMessages.Add(new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "test.v1", "test.v1", "{}", now, now));
        await db.SaveChangesAsync();
        var clock = new MutableClock(now.AddSeconds(1));
        var publisher = new TestPublisher(true);
        var processor = new ModuleOutboxProcessor<PaymentDbContext, TestPublisher>(
            new ModuleOutboxStore<PaymentDbContext>(db), publisher,
            Microsoft.Extensions.Options.Options.Create(new ModuleOutboxOptions<TestPublisher>
            { MaximumAttempts = 2, RetryJitterFactor = 0 }), clock,
            NullLogger<ModuleOutboxProcessor<PaymentDbContext, TestPublisher>>.Instance);

        await processor.ProcessBatchAsync();
        var retry = await db.OutboxMessages.AsNoTracking().SingleAsync();
        Assert.AreEqual(OutboxMessageStatus.Pending, retry.Status);
        Assert.AreEqual(5d, (retry.NextAttemptAtUtc - clock.Now).TotalSeconds, 0.001d);
        clock.Now = retry.NextAttemptAtUtc;
        await processor.ProcessBatchAsync();
        var failed = await db.OutboxMessages.AsNoTracking().SingleAsync();
        Assert.AreEqual(OutboxMessageStatus.Failed, failed.Status);
        Assert.AreEqual(2, failed.AttemptCount);
        Assert.HasCount(2, publisher.Messages);
    }

    [TestMethod]
    public async Task PaymentTransitions_PersistOutboxWithDistinctPartialRefunds()
    {
        string connection = await NewDatabaseAsync();
        await using var db = PaymentContext(connection);
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        var payment = CreatePayment();
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        Assert.AreEqual(0, await db.OutboxMessages.CountAsync());

        payment.MarkAuthorized("auth", PaymentOperationType.Authorize, "conversation", "reference");
        await db.SaveChangesAsync();
        payment.MarkCaptured("capture", "conversation", "reference");
        await db.SaveChangesAsync();
        payment.Refund("refund-1", 40m, "First partial refund", "refund-ref-1");
        await db.SaveChangesAsync();
        payment.Refund("refund-2", 60m, "Remaining refund", "refund-ref-2");
        await db.SaveChangesAsync();

        var events = (await db.OutboxMessages.AsNoTracking().ToArrayAsync())
            .Select(x => Read<PaymentStatusChangedIntegrationEvent>(x.Payload)).ToArray();
        Assert.HasCount(4, events);
        var refunds = events.Where(x => x.Kind == Payment.Contracts.IntegrationEvents.PaymentEventKind.Refunded).ToArray();
        Assert.HasCount(2, refunds);
        Assert.AreEqual(100m, refunds.Sum(x => x.Amount));
        Assert.AreNotEqual(refunds[0].RefundId, refunds[1].RefundId);
        Assert.IsTrue(events.All(x => x.RecipientEmail == "customer@example.com"));
        Assert.HasCount(0, payment.DomainEvents);
    }

    [TestMethod]
    public async Task PaymentFailure_PersistsFailureDescriptionInOutbox()
    {
        string connection = await NewDatabaseAsync();
        await using var db = PaymentContext(connection);
        await db.Database.MigrateAsync();
        var payment = CreatePayment();
        payment.MarkFailed("declined", PaymentOperationType.Authorize, "declined", "Card was declined");
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var e = Read<PaymentStatusChangedIntegrationEvent>((await db.OutboxMessages.SingleAsync()).Payload);
        Assert.AreEqual(Payment.Contracts.IntegrationEvents.PaymentEventKind.Failed, e.Kind);
        Assert.AreEqual("Card was declined", e.Description);
    }

    [TestMethod]
    public async Task PaymentRollback_DoesNotCommitBusinessStateOrOutbox()
    {
        string connection = await NewDatabaseAsync();
        await using var db = PaymentContext(connection);
        await db.Database.MigrateAsync();
        var payment = CreatePayment();
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            payment.MarkCaptured("capture", null, "reference");
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        await using var verify = PaymentContext(connection);
        Assert.AreEqual(PaymentStatus.Pending, (await verify.Payments.SingleAsync()).Status);
        Assert.AreEqual(0, await verify.OutboxMessages.CountAsync());
    }

    [TestMethod]
    public async Task FailedSave_RetainsEventAndRetryDoesNotDuplicateOutbox()
    {
        string connection = await NewDatabaseAsync();
        var interceptor = new FailFirstSave();
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(connection)
            .AddInterceptors(interceptor).Options;
        await using var db = new PaymentDbContext(options);
        await db.Database.MigrateAsync();
        var payment = CreatePayment();
        payment.MarkCaptured("capture", null, "reference");
        db.Payments.Add(payment);

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.HasCount(1, payment.DomainEvents);
        await db.SaveChangesAsync();

        Assert.AreEqual(1, await db.OutboxMessages.CountAsync());
        Assert.HasCount(0, payment.DomainEvents);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShipmentLifecycle_PersistsAllFourOutcomesAndTrackingSnapshot(bool hasContactSnapshot)
    {
        string connection = await NewDatabaseAsync();
        await using var db = ShipmentContext(connection);
        await db.Database.MigrateAsync();
        Assert.IsFalse(db.Database.HasPendingModelChanges());
        Guid customer = Guid.NewGuid();
        var shipment = ShipmentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1", "SHP-1",
            "Customer", "+90 555 000 00 00",
            ShipmentAddress.Create("Customer", "+90 555 000 00 00", "Turkey", "Istanbul", "Kadikoy", "Street", null, "34000"),
            new[] { new Shipment.Domain.Entities.ShipmentLineDraft(Guid.NewGuid(), Guid.NewGuid(), null, "Product", null, "SKU", 1) },
            customerId: hasContactSnapshot ? customer : null,
            customerEmail: hasContactSnapshot ? "snapshot@example.com" : null,
            customerFullName: hasContactSnapshot ? "Snapshot Customer" : null);
        shipment.AssignCarrier("carrier", "Carrier", null, null, "https://example.com/track");
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        Guid package = shipment.AddPackage("PKG-1", "TRACK-1", null, null, null);
        shipment.MarkShipped();
        await db.SaveChangesAsync();
        shipment.MarkDeliveryException(package, DateTime.UtcNow, "Recipient unavailable", null, null, null);
        await db.SaveChangesAsync();
        shipment.MarkDelivered();
        await db.SaveChangesAsync();

        var events = (await db.OutboxMessages.AsNoTracking()
            .Where(x => x.EventType == ShipmentStatusChangedIntegrationEvent.EventType).ToArrayAsync())
            .Select(x => Read<ShipmentStatusChangedIntegrationEvent>(x.Payload)).ToArray();
        Assert.HasCount(4, events);
        Assert.IsTrue(events.All(x => x.CustomerId == (hasContactSnapshot ? customer : null)));
        Assert.IsTrue(events.All(x => x.RecipientEmail == (hasContactSnapshot ? "snapshot@example.com" : null)));
        var shipped = events.Single(x => x.Kind == Shipment.Contracts.IntegrationEvents.ShipmentEventKind.Shipped);
        Assert.AreEqual("TRACK-1", shipped.TrackingNumber);
        Assert.AreEqual("Carrier", shipped.CarrierName);
        Assert.AreEqual("Recipient unavailable", events.Single(x =>
            x.Kind == Shipment.Contracts.IntegrationEvents.ShipmentEventKind.DeliveryException).Description);
        Assert.HasCount(0, shipment.DomainEvents);
    }

    [TestMethod]
    public async Task SharedOutbox_ConcurrentClaimsAreDisjointAndStaleOwnerCannotFinalize()
    {
        string connection = await NewDatabaseAsync();
        await using var first = PaymentContext(connection);
        await first.Database.MigrateAsync();
        var now = DateTime.UtcNow;
        for (int i = 0; i < 2; i++)
            first.OutboxMessages.Add(new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "test.v1", "test.v1", "{}", now, now));
        await first.SaveChangesAsync();
        await using var second = PaymentContext(connection);
        var store1 = new ModuleOutboxStore<PaymentDbContext>(first);
        var store2 = new ModuleOutboxStore<PaymentDbContext>(second);
        var results = await Task.WhenAll(
            store1.ClaimAsync(Guid.NewGuid(), now, now.AddMinutes(1), 1),
            store2.ClaimAsync(Guid.NewGuid(), now, now.AddMinutes(1), 1));
        Assert.AreNotEqual(results[0].Single().Id, results[1].Single().Id);

        var recovered = await store2.ClaimAsync(Guid.NewGuid(), now.AddMinutes(2), now.AddMinutes(3), 2);
        Assert.HasCount(2, recovered);
        var stale = results[0].Single();
        Assert.IsFalse(await store1.MarkPublishedAsync(stale.Id, stale.LockId, now.AddMinutes(2)));
    }

    internal static async Task<string> NewDatabaseAsync()
    {
        string name = "rollout_" + Guid.NewGuid().ToString("N");
        await using var connection = new NpgsqlConnection(PostgreSqlTestContainer.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
        await command.ExecuteNonQueryAsync();
        return new NpgsqlConnectionStringBuilder(PostgreSqlTestContainer.ConnectionString) { Database = name }.ConnectionString;
    }

    private static PaymentDbContext PaymentContext(string connection) => new(
        new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(connection).Options);
    private static ShipmentDbContext ShipmentContext(string connection) => new(
        new DbContextOptionsBuilder<ShipmentDbContext>().UseNpgsql(connection).Options);
    private static PaymentEntity CreatePayment() => PaymentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1",
        Guid.NewGuid(), 100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card, "customer@example.com", "Customer");
    private static T Read<T>(string payload) => JsonSerializer.Deserialize<T>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

    private sealed class FailFirstSave : SaveChangesInterceptor
    {
        private bool _failed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_failed) { _failed = true; throw new DbUpdateException("Simulated persistence failure."); }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class MutableClock(DateTime now) : TimeProvider
    {
        public DateTime Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => new(Now);
    }

    private sealed class TestPublisher(bool fail) : IIntegrationEventPublisher<TestPublisher>
    {
        public List<IntegrationMessage> Messages { get; } = [];
        public Task PublishAsync(IntegrationMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return fail ? Task.FromException(new IOException("Simulated broker failure.")) : Task.CompletedTask;
        }
    }

}
