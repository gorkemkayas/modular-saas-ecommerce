using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Integrations;
using Payment.Application.Payments.Commands.AuthorizePayment;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.UnitTests.Payments.Commands.AuthorizePayment;

[TestClass]
public sealed class AuthorizePaymentCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenGatewayReturnsAuthorized_UpdatesPaymentAndSyncsOrder()
    {
        var repository = new Mock<IPaymentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderContextService = new Mock<IOrderPaymentContextService>();
        var orderSyncService = new Mock<IOrderPaymentSyncService>();
        var inventoryService = new Mock<IInventoryPaymentService>();
        var paymentGateway = new Mock<IPaymentGateway>();

        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1003",
            Guid.NewGuid(),
            180m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        orderContextService
            .Setup(x => x.GetCustomerOrderContextAsync(payment.StoreId, It.IsAny<Guid>(), payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderPaymentContext(
                payment.OrderId,
                payment.StoreId,
                payment.CustomerId,
                payment.OrderNumber,
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

        repository
            .Setup(x => x.GetByOrderIdAsync(payment.StoreId, payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        paymentGateway
            .Setup(x => x.AuthorizeAsync(It.IsAny<PaymentGatewayAuthorizeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayOperationResult(
                PaymentGatewayOutcome.Authorized,
                "pay-ref-1",
                "conv-1",
                null,
                null,
                null,
                "request-1"));

        var handler = new AuthorizePaymentCommandHandler(
            repository.Object,
            unitOfWork.Object,
            orderContextService.Object,
            orderSyncService.Object,
            inventoryService.Object,
            paymentGateway.Object,
            NullLogger<AuthorizePaymentCommandHandler>.Instance);

        var result = await handler.Handle(
            new AuthorizePaymentCommand(payment.StoreId, Guid.NewGuid(), payment.OrderId, "auth-1", "127.0.0.1"),
            CancellationToken.None);

        Assert.AreEqual(PaymentStatus.Authorized, payment.Status);
        Assert.AreEqual(PaymentStatus.Authorized, result.Status);
        orderSyncService.Verify(x => x.MarkAuthorizedAsync(payment.StoreId, payment.OrderId, "pay-ref-1", It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
