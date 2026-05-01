using MediatR;

namespace Shipment.Application.Shipments.Commands.MarkShipmentReadyForDispatch;

public sealed record MarkShipmentReadyForDispatchCommand(
    Guid StoreId,
    Guid ShipmentId) : IRequest;
