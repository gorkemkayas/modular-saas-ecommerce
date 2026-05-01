using MediatR;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetMyOrderShipments;

public sealed record GetMyOrderShipmentsQuery(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId) : IRequest<IReadOnlyCollection<ShipmentSummaryDto>>;
