using Inventory.Application.Abstractions.Queries;
using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.GetInventoryMovements;

public sealed class GetInventoryMovementsQueryHandler : IRequestHandler<GetInventoryMovementsQuery, PagedResult<StockMovementDto>>
{
    private readonly IInventoryReadService _inventoryReadService;

    public GetInventoryMovementsQueryHandler(IInventoryReadService inventoryReadService)
    {
        _inventoryReadService = inventoryReadService;
    }

    public Task<PagedResult<StockMovementDto>> Handle(GetInventoryMovementsQuery query, CancellationToken cancellationToken)
    {
        return _inventoryReadService.GetMovementsAsync(
            query.StoreId,
            query.InventoryItemId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}
