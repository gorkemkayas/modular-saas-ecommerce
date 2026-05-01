using MediatR;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetStoreOrderShipments;

public sealed record GetStoreOrderShipmentsQuery(
    Guid StoreId,
    Guid OrderId) : IRequest<IReadOnlyCollection<ShipmentSummaryDto>>;
