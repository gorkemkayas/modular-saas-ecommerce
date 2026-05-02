using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.CapturePayment;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.UnitTests.Payments.Commands.CapturePayment;

[TestClass]
public sealed class CapturePaymentCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenGatewayReturnsCaptured_SavesBeforeSyncsAndNotifiesLast()
    {
        var repository = new Mock<IPaymentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderContextService = new Mock<IOrderPaymentContextService>();
        var orderSyncService = new Mock<IOrderPaymentSyncService>();
        var inventoryService = new Mock<IInventoryPaymentService>();
        var notificationService = new Mock<IPaymentNotificationService>();
        var shipmentService = new Mock<IShipmentPaymentService>();
        var paymentGateway = new Mock<IPaymentGateway>();

        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-CAPTURE-2",
            Guid.NewGuid(),
            220m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        payment.MarkAuthorized(
            "initial-auth",
            PaymentOperationType.Authorize,
            "conv-auth",
            "pay-auth");

        repository
            .Setup(x => x.GetByIdAsync(payment.StoreId, payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        orderContextService
            .Setup(x => x.GetStoreOrderContextAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(
                payment.OrderId,
                payment.StoreId,
                payment.CustomerId,
                payment.OrderNumber,
                OrderLifecycleStatus.Confirmed,
                OrderPaymentLifecycleStatus.Authorized,
                OrderFulfillmentLifecycleStatus.Unfulfilled,
                payment.Amount,
                payment.CurrencyCode,
                "res-2",
                new OrderPaymentCustomer("customer@example.com", "Jane Doe", "+905551112233"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new[]
                {
                    new OrderPaymentItem(Guid.NewGuid(), "Product 1", null, "SKU-1", 1, payment.Amount)
                }));

        paymentGateway
            .Setup(x => x.CaptureAsync(It.IsAny<PaymentGatewayCaptureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayOperationResult(
                PaymentGatewayOutcome.Captured,
                "pay-ref-2",
                "conv-2",
                null,
                null,
                null,
                "request-2"));

        var savedChanges = false;
        var orderSynced = false;
        var inventoryConfirmed = false;
        var shipmentCreated = false;

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => savedChanges = true)
            .ReturnsAsync(1);

        orderSyncService
            .Setup(x => x.MarkCapturedAsync(payment.StoreId, payment.OrderId, "pay-ref-2", It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                Assert.IsTrue(savedChanges);
                orderSynced = true;
            })
            .Returns(Task.CompletedTask);

        inventoryService
            .Setup(x => x.ConfirmDeductionAsync(payment.StoreId, "res-2", "Payment captured.", It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                Assert.IsTrue(savedChanges);
                Assert.IsTrue(orderSynced);
                inventoryConfirmed = true;
            })
            .Returns(Task.CompletedTask);

        shipmentService
            .Setup(x => x.EnsureShipmentCreatedForCapturedOrderAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                Assert.IsTrue(savedChanges);
                Assert.IsTrue(orderSynced);
                Assert.IsTrue(inventoryConfirmed);
                shipmentCreated = true;
            })
            .Returns(Task.CompletedTask);

        notificationService
            .Setup(x => x.SendPaymentCapturedAsync(
                payment.StoreId,
                payment.Id,
                payment.OrderId,
                payment.CustomerId,
                payment.OrderNumber,
                "customer@example.com",
                "Jane Doe",
                payment.Amount,
                payment.CurrencyCode,
                "pay-ref-2",
                It.IsAny<CancellationToken>()))
            .Callback(() => Assert.IsTrue(shipmentCreated))
            .Returns(Task.CompletedTask);

        var handler = new CapturePaymentCommandHandler(
            repository.Object,
            unitOfWork.Object,
            orderContextService.Object,
            orderSyncService.Object,
            inventoryService.Object,
            notificationService.Object,
            shipmentService.Object,
            paymentGateway.Object,
            NullLogger<CapturePaymentCommandHandler>.Instance);

        var result = await handler.Handle(
            new CapturePaymentCommand(payment.StoreId, payment.Id, "capture-success"),
            CancellationToken.None);

        Assert.AreEqual(Payment.Domain.Enums.PaymentStatus.Captured, payment.Status);
        Assert.AreEqual(Payment.Domain.Enums.PaymentStatus.Captured, result.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        orderSyncService.Verify(x => x.MarkCapturedAsync(payment.StoreId, payment.OrderId, "pay-ref-2", It.IsAny<CancellationToken>()), Times.Once);
        inventoryService.Verify(x => x.ConfirmDeductionAsync(payment.StoreId, "res-2", "Payment captured.", It.IsAny<CancellationToken>()), Times.Once);
        shipmentService.Verify(x => x.EnsureShipmentCreatedForCapturedOrderAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>()), Times.Once);
        notificationService.Verify(x => x.SendPaymentCapturedAsync(
            payment.StoreId,
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.OrderNumber,
            "customer@example.com",
            "Jane Doe",
            payment.Amount,
            payment.CurrencyCode,
            "pay-ref-2",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenOrderIsCancelled_DoesNotCallGateway()
    {
        var repository = new Mock<IPaymentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderContextService = new Mock<IOrderPaymentContextService>();
        var orderSyncService = new Mock<IOrderPaymentSyncService>();
        var inventoryService = new Mock<IInventoryPaymentService>();
        var notificationService = new Mock<IPaymentNotificationService>();
        var shipmentService = new Mock<IShipmentPaymentService>();
        var paymentGateway = new Mock<IPaymentGateway>();

        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-CAPTURE-1",
            Guid.NewGuid(),
            180m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        repository
            .Setup(x => x.GetByIdAsync(payment.StoreId, payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        orderContextService
            .Setup(x => x.GetStoreOrderContextAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(
                payment.OrderId,
                payment.StoreId,
                payment.CustomerId,
                payment.OrderNumber,
                OrderLifecycleStatus.Cancelled,
                OrderPaymentLifecycleStatus.Pending,
                OrderFulfillmentLifecycleStatus.Unfulfilled,
                payment.Amount,
                payment.CurrencyCode,
                "res-1",
                new OrderPaymentCustomer("customer@example.com", "Jane Doe", "+905551112233"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new[]
                {
                    new OrderPaymentItem(Guid.NewGuid(), "Product 1", null, "SKU-1", 1, payment.Amount)
                }));

        var handler = new CapturePaymentCommandHandler(
            repository.Object,
            unitOfWork.Object,
            orderContextService.Object,
            orderSyncService.Object,
            inventoryService.Object,
            notificationService.Object,
            shipmentService.Object,
            paymentGateway.Object,
            NullLogger<CapturePaymentCommandHandler>.Instance);

        await Assert.ThrowsExactlyAsync<Payment.Application.Exceptions.PaymentValidationException>(() =>
            handler.Handle(
                new CapturePaymentCommand(payment.StoreId, payment.Id, "capture-cancelled"),
                CancellationToken.None));

        paymentGateway.Verify(x => x.CaptureAsync(It.IsAny<PaymentGatewayCaptureRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
