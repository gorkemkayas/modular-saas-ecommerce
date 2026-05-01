using MediatR;

namespace Shipment.Application.Shipments.Commands.MarkShipmentDeliveryException;

public sealed record MarkShipmentDeliveryExceptionCommand(
    Guid StoreId,
    Guid ShipmentId,
    Guid PackageId,
    string Description,
    string? Location,
    string? RawStatusCode,
    string? RawStatusText) : IRequest;
