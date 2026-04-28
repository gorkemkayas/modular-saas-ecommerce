using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Order.Application.Abstractions;
using Order.Application.Integrations;
using Order.Application.Orders.Commands.PlaceOrder;
using Order.Domain.Repositories;

namespace Order.Application.UnitTests.Orders.Commands.PlaceOrder;

[TestClass]
public sealed class PlaceOrderCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WithValidInputs_PersistsOrderAndReturnsId()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var orderNumberGenerator = new Mock<IOrderNumberGenerator>();
        var customerContextService = new Mock<IOrderCustomerContextService>();
        var catalogProductService = new Mock<IOrderCatalogProductService>();
        var pricingService = new Mock<IOrderPricingService>();
        var inventoryService = new Mock<IOrderInventoryService>();

        orderNumberGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-TEST-APP-1");

        customerContextService
            .Setup(x => x.GetCustomerContextAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderCustomerContext(
                Guid.NewGuid(),
                "customer@example.com",
                "Jane Doe",
                "+90 555 000 00 00",
                "TRY",
                new OrderAddressSnapshotData("Shipping", "Jane Doe", "+90 555 000 00 00", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000"),
                new OrderAddressSnapshotData("Billing", "Jane Doe", "+90 555 000 00 00", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000")));

        catalogProductService
            .Setup(x => x.GetSellableItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid storeId, Guid productId, Guid? variantId, CancellationToken _) =>
                new OrderSellableItem(productId, variantId, "Phone", null, "SKU-1"));

        pricingService
            .Setup(x => x.ResolvePriceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResolvedOrderPrice(100m, "TRY", 120m, Guid.NewGuid(), Guid.NewGuid()));

        inventoryService
            .Setup(x => x.EnsureAvailabilityAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<OrderInventoryItemRequest>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        inventoryService
            .Setup(x => x.ReserveAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyCollection<OrderInventoryItemRequest>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new PlaceOrderCommandHandler(
            orderRepository.Object,
            unitOfWork.Object,
            orderNumberGenerator.Object,
            customerContextService.Object,
            catalogProductService.Object,
            pricingService.Object,
            inventoryService.Object,
            NullLogger<PlaceOrderCommandHandler>.Instance);

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "TRY",
            new[]
            {
                new PlaceOrderItemInput(Guid.NewGuid(), null, 2)
            });

        var orderId = await handler.Handle(command, CancellationToken.None);

        Assert.AreNotEqual(Guid.Empty, orderId);
        orderRepository.Verify(x => x.AddAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        inventoryService.Verify(x => x.EnsureAvailabilityAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<OrderInventoryItemRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        inventoryService.Verify(x => x.ReserveAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyCollection<OrderInventoryItemRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
