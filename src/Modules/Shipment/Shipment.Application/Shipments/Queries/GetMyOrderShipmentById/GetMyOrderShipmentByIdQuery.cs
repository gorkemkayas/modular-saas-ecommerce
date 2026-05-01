using MediatR;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetMyOrderShipmentById;

public sealed record GetMyOrderShipmentByIdQuery(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    Guid ShipmentId) : IRequest<ShipmentDto?>;
