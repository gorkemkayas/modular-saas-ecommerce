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
