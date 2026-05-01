using MediatR;

namespace Shipment.Application.Shipments.Commands.AssignShipmentCarrier;

public sealed record AssignShipmentCarrierCommand(
    Guid StoreId,
    Guid ShipmentId,
    string CarrierCode,
    string CarrierName,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl) : IRequest;
