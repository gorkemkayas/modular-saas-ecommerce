using MediatR;

namespace Shipment.Application.Shipments.Commands.AddShipmentPackage;

public sealed record AddShipmentPackageCommand(
    Guid StoreId,
    Guid ShipmentId,
    string? TrackingNumber,
    decimal? Weight,
    string? WeightUnit,
    string? LabelReference) : IRequest<Guid>;
