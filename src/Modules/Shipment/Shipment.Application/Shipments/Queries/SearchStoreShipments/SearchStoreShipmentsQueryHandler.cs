using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Common.Models;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.SearchStoreShipments;

public sealed class SearchStoreShipmentsQueryHandler : IRequestHandler<SearchStoreShipmentsQuery, PagedResult<ShipmentSummaryDto>>
{
    private readonly IShipmentReadService _shipmentReadService;

    public SearchStoreShipmentsQueryHandler(IShipmentReadService shipmentReadService)
    {
        _shipmentReadService = shipmentReadService;
    }

    public Task<PagedResult<ShipmentSummaryDto>> Handle(SearchStoreShipmentsQuery query, CancellationToken cancellationToken)
    {
        return _shipmentReadService.SearchAsync(
            new ShipmentSearchCriteria(
                query.StoreId,
                query.Status,
                query.OrderId,
                query.OrderNumber,
                query.ShipmentNumber,
                query.TrackingNumber,
                query.PageNumber,
                query.PageSize),
            cancellationToken);
    }
}
