using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.ProcessPaymentWebhook;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Application.UnitTests.Payments.Commands;

[TestClass]
public sealed class RefundWebhookTests
{
    [TestMethod]
    [DataRow(20)]
    [DataRow(100)]
    public async Task CallbackReplay_AcquiresLockBeforeReload_AndDoesNotCreateAnotherRefund(int amount)
    {
        var payment = PaymentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1", Guid.NewGuid(),
            100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card);
        payment.MarkCaptured("capture", null, "ref");
        payment.Refund("refund", amount, "Command reason", "ref");
        payment.ClearDomainEvents();
        var repository = new Mock<IPaymentRepository>(MockBehavior.Strict);
        var refundLock = new Mock<IPaymentRefundLock>(MockBehavior.Strict);
        var lease = new TestLease();
        var sequence = new MockSequence();
        repository.InSequence(sequence).Setup(x => x.GetSnapshotByProviderReferenceAsync(PaymentProvider.Mock,
            null, "ref", "refund", CancellationToken.None)).ReturnsAsync(payment);
        refundLock.InSequence(sequence).Setup(x => x.AcquireAsync(payment.StoreId, payment.Id, CancellationToken.None))
            .ReturnsAsync(lease);
        repository.InSequence(sequence).Setup(x => x.GetByIdAsync(payment.StoreId, payment.Id, CancellationToken.None))
            .ReturnsAsync(payment);
        var parser = new Mock<IPaymentWebhookParser>();
        parser.Setup(x => x.ParseAsync(PaymentProvider.Mock, "payload", "signature", CancellationToken.None))
            .ReturnsAsync(new ParsedPaymentWebhook(PaymentProvider.Mock, PaymentGatewayOutcome.Refunded,
                "refund", "ref", null, null, null, null));
        var work = new Mock<IUnitOfWork>();
        bool saved = false;
        work.Setup(x => x.SaveChangesAsync(CancellationToken.None)).Callback(() => saved = true).ReturnsAsync(1);
        var sync = new Mock<IOrderPaymentSyncService>();
        sync.Setup(x => x.MarkRefundedAsync(payment.StoreId, payment.OrderId, "ref", CancellationToken.None))
            .Callback(() => Assert.IsTrue(saved)).Returns(Task.CompletedTask);
        var handler = new ProcessPaymentWebhookCommandHandler(repository.Object, work.Object,
            Mock.Of<IOrderPaymentContextService>(), sync.Object, Mock.Of<IInventoryPaymentService>(),
            Mock.Of<IShipmentPaymentService>(), parser.Object, Mock.Of<IPaymentWebhookSignatureValidator>(),
            refundLock.Object, NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        await handler.Handle(new(PaymentProvider.Mock, "payload", "signature"), CancellationToken.None);

        Assert.AreEqual((decimal)amount, payment.RefundedAmount);
        Assert.HasCount(1, payment.Refunds);
        Assert.HasCount(0, payment.DomainEvents);
        sync.Verify(x => x.MarkRefundedAsync(payment.StoreId, payment.OrderId, "ref", CancellationToken.None),
            amount == 100 ? Times.Once() : Times.Never());
        Assert.IsTrue(lease.Disposed);
    }

    private sealed class TestLease : IAsyncDisposable
    {
        public bool Disposed { get; private set; }
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
}
