using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Order.Application.Abstractions;
using Order.Contracts;
using Order.Domain.Events;
using Order.Application.Integrations;
using Order.Application.Orders.Commands.CancelOrder;
using Order.Domain.Exceptions;
using Order.Domain.Entities;
using Order.Domain.Models;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.UnitTests.Orders.Commands.CancelOrder;

[TestClass]
public sealed class CancelOrderCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenOrderHasReservation_PersistsReservationCancellationEvent()
    {
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Domain.Entities.Order.Place(
            storeId,
            OrderNumber.Create("ORD-CANCEL-1"),
            CustomerSnapshot.Create(customerId, "customer@example.com", "Jane Doe", "+90 555 000 00 00"),
            CreateAddress("Billing"),
            CreateAddress("Shipping"),
            "TRY",
            new[]
            {
                new OrderItemDraft(
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1,
                    OrderPriceSnapshot.Create(100m, "TRY", 120m, Guid.NewGuid(), Guid.NewGuid()))
            });

        order.SetReservationReference("ord-res-1");

        var orderRepository = new Mock<IOrderRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var customerContextService = new Mock<IOrderCustomerContextService>();

        customerContextService
            .Setup(x => x.GetCustomerIdentityAsync(storeId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderCustomerIdentity(customerId));

        orderRepository
            .Setup(x => x.GetByIdAsync(storeId, order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);


        var handler = new CancelOrderCommandHandler(
            orderRepository.Object,
            unitOfWork.Object,
            customerContextService.Object,
            Mock.Of<IOrderOperationLock>(),
            Mock.Of<IOrderCancellationPaymentGuard>(),
            NullLogger<CancelOrderCommandHandler>.Instance);

        await handler.Handle(new CancelOrderCommand(storeId, Guid.NewGuid(), order.Id, "Customer changed mind"), CancellationToken.None);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual("ord-res-1", order.DomainEvents.OfType<OrderReservationCancelledDomainEvent>().Single().ReservationReference);
    }

    [TestMethod]
    public async Task Handle_WhenOrderPaymentCaptured_ThrowsAndDoesNotReleaseReservation()
    {
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Domain.Entities.Order.Place(
            storeId,
            OrderNumber.Create("ORD-CANCEL-2"),
            CustomerSnapshot.Create(customerId, "customer@example.com", "Jane Doe", "+90 555 000 00 00"),
            CreateAddress("Billing"),
            CreateAddress("Shipping"),
            "TRY",
            new[]
            {
                new OrderItemDraft(
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1,
                    OrderPriceSnapshot.Create(100m, "TRY", 120m, Guid.NewGuid(), Guid.NewGuid()))
            });

        order.SetReservationReference("ord-res-2");
        order.MarkPaymentCaptured("PAY-1");

        var orderRepository = new Mock<IOrderRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var customerContextService = new Mock<IOrderCustomerContextService>();

        customerContextService
            .Setup(x => x.GetCustomerIdentityAsync(storeId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderCustomerIdentity(customerId));

        orderRepository
            .Setup(x => x.GetByIdAsync(storeId, order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new CancelOrderCommandHandler(
            orderRepository.Object,
            unitOfWork.Object,
            customerContextService.Object,
            Mock.Of<IOrderOperationLock>(),
            Mock.Of<IOrderCancellationPaymentGuard>(),
            NullLogger<CancelOrderCommandHandler>.Instance);

        await Assert.ThrowsExactlyAsync<OrderDomainException>(() =>
            handler.Handle(new CancelOrderCommand(storeId, Guid.NewGuid(), order.Id, "Customer changed mind"), CancellationToken.None));

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static OrderAddressSnapshot CreateAddress(string title)
    {
        return OrderAddressSnapshot.Create(
            title,
            "Jane Doe",
            "+90 555 000 00 00",
            "Turkey",
            "Istanbul",
            "Kadikoy",
            "Street 1",
            null,
            "34000");
    }
}
