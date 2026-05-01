using MediatR;
using Moq;
using Shipment.Application.Shipments.Commands.CreateShipment;
using Shipment.Application.Shipments.Commands.EnsureShipmentCreatedForCapturedOrder;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;
using Shipment.Domain.ValueObjects;

namespace Shipment.Application.UnitTests.Shipments.Commands.EnsureShipmentCreatedForCapturedOrder;

[TestClass]
public sealed class EnsureShipmentCreatedForCapturedOrderCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenActiveShipmentAlreadyExists_ReturnsExistingShipmentId()
    {
        var repository = new Mock<IShipmentRepository>();
        var sender = new Mock<ISender>();

        var shipment = Shipment.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-2001",
            "SHP-2001",
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
                new ShipmentLineDraft(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1)
            });

        repository
            .Setup(x => x.GetActiveForOrderAsync(shipment.StoreId, shipment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var handler = new EnsureShipmentCreatedForCapturedOrderCommandHandler(
            repository.Object,
            sender.Object);

        var shipmentId = await handler.Handle(
            new EnsureShipmentCreatedForCapturedOrderCommand(shipment.StoreId, shipment.OrderId, null),
            CancellationToken.None);

        Assert.AreEqual(shipment.Id, shipmentId);
        sender.Verify(x => x.Send(It.IsAny<CreateShipmentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
