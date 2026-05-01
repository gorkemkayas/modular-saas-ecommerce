using MediatR;

namespace Shipment.Application.Shipments.Commands.CancelShipment;

public sealed record CancelShipmentCommand(
    Guid StoreId,
    Guid ShipmentId,
    string? Reason) : IRequest;
