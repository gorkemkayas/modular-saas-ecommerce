using Moq;
using Shipment.Application.Abstractions;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.Commands.CancelShipment;
using Shipment.Domain.Repositories;
using Shipment.Domain.Entities;
using Shipment.Domain.ValueObjects;

namespace Shipment.Application.UnitTests.Shipments.Commands.CancelShipment;

[TestClass]
public sealed class CancelShipmentCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_PersistsShipmentChange()
    {
        var repository = new Mock<IShipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var shipment = Shipment.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-3003",
            "SHP-3003",
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

        repository
            .Setup(x => x.GetByIdAsync(shipment.StoreId, shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))

            .ReturnsAsync(1);

        var handler = new CancelShipmentCommandHandler(
            repository.Object,
            unitOfWork.Object);

        await handler.Handle(new CancelShipmentCommand(shipment.StoreId, shipment.Id, "Customer requested cancellation"), CancellationToken.None);

        Assert.AreEqual(Shipment.Domain.Enums.ShipmentStatus.Cancelled, shipment.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
