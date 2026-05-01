using MediatR;

namespace Shipment.Application.Shipments.Commands.MarkShipmentDelivered;

public sealed record MarkShipmentDeliveredCommand(
    Guid StoreId,
    Guid ShipmentId) : IRequest;
