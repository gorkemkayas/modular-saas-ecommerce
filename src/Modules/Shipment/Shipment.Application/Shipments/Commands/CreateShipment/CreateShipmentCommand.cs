using MediatR;

namespace Shipment.Application.Shipments.Commands.CreateShipment;

public sealed record CreateShipmentCommand(
    Guid StoreId,
    Guid OrderId,
    string? InternalNote) : IRequest<Guid>;
