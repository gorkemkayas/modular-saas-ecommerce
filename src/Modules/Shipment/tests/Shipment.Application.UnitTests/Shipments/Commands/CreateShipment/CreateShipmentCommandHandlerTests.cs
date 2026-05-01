using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shipment.Application.Abstractions;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.Commands.CreateShipment;
using Shipment.Domain.Repositories;

namespace Shipment.Application.UnitTests.Shipments.Commands.CreateShipment;

[TestClass]
public sealed class CreateShipmentCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WithEligibleOrder_CreatesShipmentAndSyncsOrder()
    {
        var repository = new Mock<IShipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var numberGenerator = new Mock<IShipmentNumberGenerator>();
        var orderContextService = new Mock<IOrderShipmentContextService>();
        var orderSyncService = new Mock<IOrderShipmentSyncService>();
        var notificationService = new Mock<IShipmentNotificationService>();

        var storeId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        numberGenerator
            .Setup(x => x.GenerateAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("SHP-TEST-001");

        orderContextService
            .Setup(x => x.GetStoreOrderContextAsync(storeId, orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderShipmentContext(
                orderId,
                storeId,
                Guid.NewGuid(),
                "ORD-1001",
                "customer@example.com",
                "Jane Doe",
                OrderShipmentStatus.Confirmed,
                OrderShipmentPaymentStatus.Captured,
                OrderShipmentFulfillmentStatus.Unfulfilled,
                null,
                new OrderShipmentAddress(
                    "Jane Doe",
                    "+90 555 000 00 00",
                    "Turkey",
                    "Istanbul",
                    "Kadikoy",
                    "Street 1",
                    null,
                    "34000"),
                new[]
                {
                    new OrderShipmentItem(Guid.NewGuid(), Guid.NewGuid(), null, "Phone", null, "SKU-1", 1)
                }));

        var handler = new CreateShipmentCommandHandler(
            repository.Object,
            unitOfWork.Object,
            numberGenerator.Object,
            orderContextService.Object,
            orderSyncService.Object,
            notificationService.Object,
            NullLogger<CreateShipmentCommandHandler>.Instance);

        var shipmentId = await handler.Handle(new CreateShipmentCommand(storeId, orderId, "Fragile"), CancellationToken.None);

        Assert.AreNotEqual(Guid.Empty, shipmentId);
        repository.Verify(x => x.AddAsync(It.IsAny<Shipment.Domain.Entities.Shipment>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        orderSyncService.Verify(x => x.MarkShipmentCreatedAsync(storeId, orderId, "SHP-TEST-001", It.IsAny<CancellationToken>()), Times.Once);
    }
}
