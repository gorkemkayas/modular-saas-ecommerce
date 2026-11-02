using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.RefundPayment;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;
using Payment.Domain.Repositories;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Application.UnitTests.Payments.Commands;

[TestClass]
public sealed class RefundPaymentCommandHandlerTests
{
    [TestMethod]
    [DataRow(20)]
    [DataRow(100)]
    public async Task Replay_ReturnsExistingResultWithoutGatewayOrSave(int amount)
    {
        var test = new Fixture();
        test.Payment.Refund("refund", amount, "Reason", "ref");
        await test.Handler.Handle(test.Command(amount), CancellationToken.None);

        test.Gateway.VerifyNoOtherCalls();
        test.Work.VerifyNoOtherCalls();
        test.Sync.Verify(x => x.MarkRefundedAsync(test.Payment.StoreId, test.Payment.OrderId, "ref", CancellationToken.None),
            amount == 100 ? Times.Once() : Times.Never());
        test.Sync.VerifyNoOtherCalls();
        Assert.IsTrue(test.Lease.Disposed);
    }

    [TestMethod]
    [DataRow(120)]
    [DataRow(1.001)]
    public async Task InvalidAmount_DoesNotReachProvider(double amount)
    {
        var test = new Fixture();
        await Assert.ThrowsExactlyAsync<PaymentDomainException>(() => test.Handler.Handle(test.Command((decimal)amount), CancellationToken.None));
        test.Gateway.VerifyNoOtherCalls();
        test.Work.VerifyNoOtherCalls();
        Assert.IsTrue(test.Lease.Disposed);
    }

    [TestMethod]
    public async Task Success_PersistsLocalResultBeforeOrderSync_AndReplayRepairsFailedSync()
    {
        var test = new Fixture();
        bool saved = false;
        test.Work.Setup(x => x.SaveChangesAsync(CancellationToken.None)).Callback(() => saved = true).ReturnsAsync(1);
        test.Gateway.Setup(x => x.RefundAsync(It.IsAny<PaymentGatewayRefundRequest>(), CancellationToken.None))
            .ReturnsAsync(new PaymentGatewayOperationResult(PaymentGatewayOutcome.Refunded, "ref", null, null, null, null, "request"));
        test.Sync.Setup(x => x.MarkRefundedAsync(test.Payment.StoreId, test.Payment.OrderId, "ref", CancellationToken.None))
            .Callback(() => Assert.IsTrue(saved)).ThrowsAsync(new IOException("Order unavailable"));

        await Assert.ThrowsExactlyAsync<IOException>(() => test.Handler.Handle(test.Command(100m), CancellationToken.None));
        Assert.AreEqual(PaymentStatus.Refunded, test.Payment.Status);
        test.Sync.Setup(x => x.MarkRefundedAsync(test.Payment.StoreId, test.Payment.OrderId, "ref", CancellationToken.None))
            .Returns(Task.CompletedTask);
        await test.Handler.Handle(test.Command(100m), CancellationToken.None);

        test.Gateway.Verify(x => x.RefundAsync(It.IsAny<PaymentGatewayRefundRequest>(), CancellationToken.None), Times.Once);
        test.Work.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        Assert.HasCount(1, test.Payment.Refunds);
    }

    [TestMethod]
    public async Task RejectedRefund_SavesAttemptWithoutFailingPaymentOrSynchronizingOrder()
    {
        var test = new Fixture();
        test.Gateway.Setup(x => x.RefundAsync(It.IsAny<PaymentGatewayRefundRequest>(), CancellationToken.None))
            .ReturnsAsync(new PaymentGatewayOperationResult(PaymentGatewayOutcome.Failed, "ref", null, null, "declined", "Declined", "request"));

        await test.Handler.Handle(test.Command(20m), CancellationToken.None);

        Assert.AreEqual(PaymentStatus.Captured, test.Payment.Status);
        Assert.HasCount(0, test.Payment.DomainEvents);
        test.Work.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        test.Sync.VerifyNoOtherCalls();
    }

    private sealed class Fixture
    {
        public PaymentEntity Payment { get; } = PaymentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1", Guid.NewGuid(),
            100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card);
        public Mock<IPaymentGateway> Gateway { get; } = new(MockBehavior.Strict);
        public Mock<IUnitOfWork> Work { get; } = new();
        public Mock<IOrderPaymentSyncService> Sync { get; } = new();
        public TestLease Lease { get; } = new();
        public RefundPaymentCommandHandler Handler { get; }
        public Fixture()
        {
            Payment.MarkCaptured("capture", null, "ref");
            Payment.ClearDomainEvents();
            var repository = new Mock<IPaymentRepository>();
            repository.Setup(x => x.GetByIdAsync(Payment.StoreId, Payment.Id, CancellationToken.None)).ReturnsAsync(Payment);
            var gate = new Mock<IPaymentRefundLock>();
            gate.Setup(x => x.AcquireAsync(Payment.StoreId, Payment.Id, CancellationToken.None)).ReturnsAsync(Lease);
            Handler = new(repository.Object, Work.Object, Sync.Object, Gateway.Object, gate.Object,
                NullLogger<RefundPaymentCommandHandler>.Instance);
        }
        public RefundPaymentCommand Command(decimal amount) => new(Payment.StoreId, Payment.Id, amount, "Reason", "refund");
    }

    private sealed class TestLease : IAsyncDisposable
    {
        public bool Disposed { get; private set; }
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
}
