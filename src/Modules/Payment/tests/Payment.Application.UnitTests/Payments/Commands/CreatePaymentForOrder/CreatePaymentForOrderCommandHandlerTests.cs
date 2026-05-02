using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.CreatePaymentForOrder;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.UnitTests.Payments.Commands.CreatePaymentForOrder;

[TestClass]
public sealed class CreatePaymentForOrderCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenPaymentAlreadyExists_ReturnsExistingId()
    {
        var repository = new Mock<IPaymentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderContextService = new Mock<IOrderPaymentContextService>();
        var paymentGateway = new Mock<IPaymentGateway>();

        var existingPayment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-EXISTING",
            Guid.NewGuid(),
            100m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        orderContextService
            .Setup(x => x.GetCustomerOrderContextAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(
                existingPayment.OrderId,
                existingPayment.StoreId,
                existingPayment.CustomerId,
                existingPayment.OrderNumber,
                OrderLifecycleStatus.Confirmed,
                OrderPaymentLifecycleStatus.Pending,
                OrderFulfillmentLifecycleStatus.Unfulfilled,
                existingPayment.Amount,
                existingPayment.CurrencyCode,
                "res-1",
                new OrderPaymentCustomer("customer@example.com", "Jane Doe", "+905551112233"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new[]
                {
                    new OrderPaymentItem(Guid.NewGuid(), "Product 1", null, "SKU-1", 1, existingPayment.Amount)
                }));

        repository
            .Setup(x => x.GetByOrderIdAsync(existingPayment.StoreId, existingPayment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        paymentGateway.SetupGet(x => x.Provider).Returns(PaymentProvider.Mock);

        var handler = new CreatePaymentForOrderCommandHandler(
            repository.Object,
            unitOfWork.Object,
            orderContextService.Object,
            paymentGateway.Object,
            NullLogger<CreatePaymentForOrderCommandHandler>.Instance);

        var result = await handler.Handle(
            new CreatePaymentForOrderCommand(existingPayment.StoreId, Guid.NewGuid(), existingPayment.OrderId, PaymentMethodType.Card),
            CancellationToken.None);

        Assert.AreEqual(existingPayment.Id, result);
        repository.Verify(x => x.AddAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenOrderIsCancelled_ThrowsValidationExceptionWithoutCreatingPayment()
    {
        var repository = new Mock<IPaymentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderContextService = new Mock<IOrderPaymentContextService>();
        var paymentGateway = new Mock<IPaymentGateway>();

        var storeId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        orderContextService
            .Setup(x => x.GetCustomerOrderContextAsync(storeId, It.IsAny<Guid>(), orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(
                orderId,
                storeId,
                Guid.NewGuid(),
                "ORD-CANCELLED",
                OrderLifecycleStatus.Cancelled,
                OrderPaymentLifecycleStatus.Pending,
                OrderFulfillmentLifecycleStatus.Unfulfilled,
                100m,
                "TRY",
                "res-2",
                new OrderPaymentCustomer("customer@example.com", "Jane Doe", "+905551112233"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new OrderPaymentAddress("Jane Doe", "+905551112233", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new[]
                {
                    new OrderPaymentItem(Guid.NewGuid(), "Product 1", null, "SKU-1", 1, 100m)
                }));

        paymentGateway.SetupGet(x => x.Provider).Returns(PaymentProvider.Mock);

        var handler = new CreatePaymentForOrderCommandHandler(
            repository.Object,
            unitOfWork.Object,
            orderContextService.Object,
            paymentGateway.Object,
            NullLogger<CreatePaymentForOrderCommandHandler>.Instance);

        await Assert.ThrowsExactlyAsync<Payment.Application.Exceptions.PaymentValidationException>(() =>
            handler.Handle(
                new CreatePaymentForOrderCommand(storeId, Guid.NewGuid(), orderId, PaymentMethodType.Card),
                CancellationToken.None));

        repository.Verify(x => x.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(x => x.AddAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
