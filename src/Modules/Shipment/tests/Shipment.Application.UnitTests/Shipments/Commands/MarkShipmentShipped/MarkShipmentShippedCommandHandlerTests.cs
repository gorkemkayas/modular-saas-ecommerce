using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shipment.Application.Abstractions;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.Commands.MarkShipmentShipped;
using Shipment.Domain.Repositories;
using Shipment.Domain.Entities;
using Shipment.Domain.ValueObjects;

namespace Shipment.Application.UnitTests.Shipments.Commands.MarkShipmentShipped;

[TestClass]
public sealed class MarkShipmentShippedCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_PersistsShipmentChange()
    {
        var repository = new Mock<IShipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var shipment = Shipment.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-3001",
            "SHP-3001",
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

        shipment.AssignCarrier("yurtici", "Yurtici Kargo", null, null, "https://tracking.example/1");
        shipment.AddPackage("PKG-1", "TRK-1", 1.25m, "kg", null);

        repository
            .Setup(x => x.GetByIdAsync(shipment.StoreId, shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))

            .ReturnsAsync(1);

        var handler = new MarkShipmentShippedCommandHandler(
            repository.Object,
            unitOfWork.Object,
            NullLogger<MarkShipmentShippedCommandHandler>.Instance);

        await handler.Handle(new MarkShipmentShippedCommand(shipment.StoreId, shipment.Id), CancellationToken.None);

        Assert.AreEqual(Shipment.Domain.Enums.ShipmentStatus.Shipped, shipment.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
