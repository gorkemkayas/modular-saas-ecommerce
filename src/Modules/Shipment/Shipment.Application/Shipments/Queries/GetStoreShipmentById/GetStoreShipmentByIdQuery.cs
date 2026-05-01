using MediatR;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetStoreShipmentById;

public sealed record GetStoreShipmentByIdQuery(
    Guid StoreId,
    Guid ShipmentId) : IRequest<ShipmentDto?>;
