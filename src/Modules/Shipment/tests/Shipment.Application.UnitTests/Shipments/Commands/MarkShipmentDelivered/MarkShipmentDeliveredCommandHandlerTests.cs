using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shipment.Application.Abstractions;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.Commands.MarkShipmentDelivered;
using Shipment.Domain.Repositories;
using Shipment.Domain.Entities;
using Shipment.Domain.ValueObjects;

namespace Shipment.Application.UnitTests.Shipments.Commands.MarkShipmentDelivered;

[TestClass]
public sealed class MarkShipmentDeliveredCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_PersistsShipmentChange()
    {
        var repository = new Mock<IShipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var shipment = Shipment.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-3002",
            "SHP-3002",
            "Jane Doe",
            "+90 555 000 00 00",
            ShipmentAddress.Create(
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
                new ShipmentLineDraft(Guid.NewGuid(), Guid.NewGuid(), null, "Phone", null, "SKU-1", 1)
            });

        shipment.AssignCarrier("yurtici", "Yurtici Kargo", null, null, "https://tracking.example/2");
        shipment.AddPackage("PKG-2", "TRK-2", 1.25m, "kg", null);
        shipment.MarkShipped();

        repository
            .Setup(x => x.GetByIdAsync(shipment.StoreId, shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))

            .ReturnsAsync(1);

        var handler = new MarkShipmentDeliveredCommandHandler(
            repository.Object,
            unitOfWork.Object,
            NullLogger<MarkShipmentDeliveredCommandHandler>.Instance);

        await handler.Handle(new MarkShipmentDeliveredCommand(shipment.StoreId, shipment.Id), CancellationToken.None);

        Assert.AreEqual(Shipment.Domain.Enums.ShipmentStatus.Delivered, shipment.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
