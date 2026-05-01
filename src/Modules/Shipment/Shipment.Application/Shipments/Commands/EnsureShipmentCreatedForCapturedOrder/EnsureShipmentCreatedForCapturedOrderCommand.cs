using MediatR;

namespace Shipment.Application.Shipments.Commands.EnsureShipmentCreatedForCapturedOrder;

public sealed record EnsureShipmentCreatedForCapturedOrderCommand(
    Guid StoreId,
    Guid OrderId,
    string? InternalNote) : IRequest<Guid>;
