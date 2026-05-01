using MediatR;
using Shipment.Domain.Enums;

namespace Shipment.Application.Shipments.Commands.RegisterShipmentTrackingEvent;

public sealed record RegisterShipmentTrackingEventCommand(
    Guid StoreId,
    Guid ShipmentId,
    Guid PackageId,
    TrackingEventType Type,
    DateTime OccurredAtUtc,
    string? Location,
    string Description,
    string? RawStatusCode,
    string? RawStatusText) : IRequest;
