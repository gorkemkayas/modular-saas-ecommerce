using MediatR;
using Shipment.Application.Common.Models;
using Shipment.Application.Shipments.DTOs;
using Shipment.Domain.Enums;

namespace Shipment.Application.Shipments.Queries.SearchStoreShipments;

public sealed record SearchStoreShipmentsQuery(
    Guid StoreId,
    ShipmentStatus? Status,
    Guid? OrderId,
    string? OrderNumber,
    string? ShipmentNumber,
    string? TrackingNumber,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<ShipmentSummaryDto>>;
