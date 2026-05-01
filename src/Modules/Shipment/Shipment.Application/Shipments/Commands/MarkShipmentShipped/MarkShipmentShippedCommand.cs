using MediatR;

namespace Shipment.Application.Shipments.Commands.MarkShipmentShipped;

public sealed record MarkShipmentShippedCommand(
    Guid StoreId,
    Guid ShipmentId) : IRequest;
