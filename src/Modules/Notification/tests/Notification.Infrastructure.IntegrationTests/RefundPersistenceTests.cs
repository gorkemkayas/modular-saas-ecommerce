using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payment.Application.Exceptions;
using Payment.Contracts.IntegrationEvents;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Options;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Repositories;
using Payment.Infrastructure.Gateways;
using PaymentEntity = Payment.Domain.Entities.Payment;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class RefundPersistenceTests
{
    [TestMethod]
    public async Task RefundWebhook_RequiresStableOperationKey_AndPrefersExplicitCommandKey()
    {
        var parser = new PaymentWebhookParser();
        await Assert.ThrowsExactlyAsync<PaymentWebhookValidationException>(() => parser.ParseAsync(PaymentProvider.Mock,
            """{"status":"refunded","iyziPaymentId":"same-payment-reference","refundAmount":20}""", null));
        var parsed = await parser.ParseAsync(PaymentProvider.Mock,
            """{"status":"refunded","idempotencyKey":"command-refund","eventId":"callback-event","refundAmount":20}""", null);
        Assert.AreEqual("command-refund", parsed.IdempotencyKey);
    }

    [TestMethod]
    public async Task RefundReplay_AfterReload_PreservesOneOutboxEvent_AndFailureDoesNotAddPaymentFailed()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        Guid store = Guid.NewGuid();
        Guid paymentId;
        await using (var db = Context(connection))
        {
            await db.Database.MigrateAsync();
            var payment = CreatePayment(store);
            paymentId = payment.Id;
            payment.MarkCaptured("capture", null, "same-payment-ref");
            payment.Refund("refund", 20m, "Reason", "same-payment-ref");
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }
        await using (var db = Context(connection))
        {
            var payment = (await new PaymentRepository(db).GetByIdAsync(store, paymentId))!;
            payment.Refund("refund", 20m, "Callback description", "same-payment-ref");
            payment.RecordRefundFailure("rejected", "declined", "Declined");
            await db.SaveChangesAsync();
            Assert.AreEqual(20m, payment.RefundedAmount);
            Assert.AreEqual(PaymentStatus.PartiallyRefunded, payment.Status);
            Assert.AreEqual(1, await db.PaymentRefunds.CountAsync());
            Assert.AreEqual(2, await db.OutboxMessages.CountAsync());
        }
    }

    [TestMethod]
    public async Task RefundKey_UniqueIndexRejectsDuplicate_ButAllowsLegacyNulls()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = Context(connection);
        await db.Database.MigrateAsync();
        var payment = CreatePayment(Guid.NewGuid());
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        db.PaymentRefunds.Add(PaymentRefund.Create(payment.Id, 10m, "Legacy 1", "same-ref"));
        db.PaymentRefunds.Add(PaymentRefund.Create(payment.Id, 10m, "Legacy 2", "same-ref"));
        db.PaymentRefunds.Add(PaymentRefund.Create(payment.Id, 10m, "Keyed", "same-ref", "unique"));
        await db.SaveChangesAsync();
        db.PaymentRefunds.Add(PaymentRefund.Create(payment.Id, 10m, "Duplicate", "same-ref", "unique"));
        var exception = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.AreEqual(PostgresErrorCodes.UniqueViolation, ((PostgresException)exception.InnerException!).SqlState);
    }

    [TestMethod]
    public async Task IndependentRefundLocks_RejectConcurrentSamePayment_AndReleaseAfterDispose()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        var options = OptionsFactory.Create(new PaymentDatabaseOptions { ConnectionString = connection });
        var firstInstance = new PostgreSqlPaymentRefundLock(options);
        var secondInstance = new PostgreSqlPaymentRefundLock(options);
        Guid store = Guid.NewGuid(), payment = Guid.NewGuid();
        await using (await firstInstance.AcquireAsync(store, payment))
        {
            await Assert.ThrowsExactlyAsync<PaymentOperationInProgressException>(() => secondInstance.AcquireAsync(store, payment));
            await using var unrelated = await secondInstance.AcquireAsync(store, Guid.NewGuid());
        }
        await using var recovered = await secondInstance.AcquireAsync(store, payment);
    }

    [TestMethod]
    public async Task LegacyPaymentWithoutContact_StillCommitsBusinessStateAndOutboxLocally()
    {
        string connection = await ProducerOutboxRolloutTests.NewDatabaseAsync();
        await using var db = Context(connection);
        await db.Database.MigrateAsync();
        var payment = CreatePayment(Guid.NewGuid());
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        payment.MarkCaptured("capture", null, "ref");
        await db.SaveChangesAsync();
        var e = JsonSerializer.Deserialize<PaymentStatusChangedIntegrationEvent>(
            (await db.OutboxMessages.SingleAsync()).Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.IsNull(e.RecipientEmail);
        Assert.AreEqual(PaymentStatus.Captured, (await db.Payments.SingleAsync()).Status);
    }

    private static PaymentDbContext Context(string connection) => new(new DbContextOptionsBuilder<PaymentDbContext>()
        .UseNpgsql(connection).Options);
    private static PaymentEntity CreatePayment(Guid store) => PaymentEntity.Create(store, Guid.NewGuid(), "ORD-REFUND",
        Guid.NewGuid(), 100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card);
}
