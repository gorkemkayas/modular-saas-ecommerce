using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.AuthorizePayment;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.UnitTests.Payments.Commands.AuthorizePayment;

[TestClass]
public sealed class PaymentCancellationBoundaryTests
{
    [TestMethod]
    public async Task ProviderTimeout_LeavesPersistedFenceAndDoesNotReportPaymentFailed()
    {
        var h = Harness();
        bool fencePersisted = false;
        h.Unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { Assert.IsTrue(h.Payment.HasProviderAttempt); fencePersisted = true; }).ReturnsAsync(1);
        h.Gateway.Setup(x => x.AuthorizeAsync(It.IsAny<PaymentGatewayAuthorizeRequest>(), It.IsAny<CancellationToken>()))
            .Callback(() => Assert.IsTrue(fencePersisted))
            .ThrowsAsync(new IOException("Provider response was lost after submission."));

        await Assert.ThrowsExactlyAsync<IOException>(() => h.Handler.Handle(h.Command, CancellationToken.None));

        Assert.AreEqual(PaymentStatus.Pending, h.Payment.Status);
        Assert.IsTrue(h.Payment.HasProviderAttempt);
        h.Sync.Verify(x => x.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task FencePersistenceFailure_DoesNotCallProvider()
    {
        var h = Harness();
        h.Unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Database unavailable"));
        await Assert.ThrowsExactlyAsync<IOException>(() => h.Handler.Handle(h.Command, CancellationToken.None));
        h.Gateway.Verify(x => x.AuthorizeAsync(It.IsAny<PaymentGatewayAuthorizeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task CancellationOwnsOrderLock_AuthorizationStopsBeforeProviderAndPersistence()
    {
        var h = Harness();
        h.Gate.Setup(x => x.AcquireAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Order operation is already owned."));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => h.Handler.Handle(h.Command, CancellationToken.None));
        h.Gateway.Verify(x => x.AuthorizeAsync(It.IsAny<PaymentGatewayAuthorizeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        h.Unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static TestHarness Harness()
    {
        var payment = global::Payment.Domain.Entities.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-1",
            Guid.NewGuid(), 100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card);
        var repository = new Mock<IPaymentRepository>();
        repository.Setup(x => x.GetByOrderIdAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        var context = new Mock<IOrderPaymentContextService>();
        context.Setup(x => x.GetCustomerOrderContextAsync(payment.StoreId, It.IsAny<Guid>(), payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(payment.OrderId, payment.StoreId, payment.CustomerId, payment.OrderNumber,
                OrderLifecycleStatus.Confirmed, OrderPaymentLifecycleStatus.Pending, OrderFulfillmentLifecycleStatus.Unfulfilled,
                100m, "TRY", "RES-1", new OrderPaymentCustomer("test@example.com", "Customer", "+905550000000"),
                Address(), Address(), [new OrderPaymentItem(Guid.NewGuid(), "Product", null, "SKU", 1, 100m)]));
        var unit = new Mock<IUnitOfWork>();
        var gateway = new Mock<IPaymentGateway>();
        var gate = new Mock<IOrderPaymentOperationLock>();
        var sync = new Mock<IOrderPaymentSyncService>();
        var handler = new AuthorizePaymentCommandHandler(repository.Object, unit.Object, context.Object, sync.Object,
            Mock.Of<IInventoryPaymentService>(), Mock.Of<IShipmentPaymentService>(), gateway.Object, gate.Object,
            NullLogger<AuthorizePaymentCommandHandler>.Instance);
        return new(payment, unit, gateway, gate, sync, handler,
            new AuthorizePaymentCommand(payment.StoreId, Guid.NewGuid(), payment.OrderId, "stable-key", "127.0.0.1"));
    }

    private static OrderPaymentAddress Address() => new("Customer", "+905550000000", "Turkey", "Istanbul", "Kadikoy", "Street", null, "34000");
    private sealed record TestHarness(global::Payment.Domain.Entities.Payment Payment, Mock<IUnitOfWork> Unit,
        Mock<IPaymentGateway> Gateway, Mock<IOrderPaymentOperationLock> Gate, Mock<IOrderPaymentSyncService> Sync,
        AuthorizePaymentCommandHandler Handler, AuthorizePaymentCommand Command);
}
