using Inventory.Application.Abstractions.Queries;
using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.GetInventoryItemById;

public sealed class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto?>
{
    private readonly IInventoryReadService _inventoryReadService;

    public GetInventoryItemByIdQueryHandler(IInventoryReadService inventoryReadService)
    {
        _inventoryReadService = inventoryReadService;
    }

    public Task<InventoryItemDto?> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken)
    {
        return _inventoryReadService.GetByIdAsync(query.StoreId, query.InventoryItemId, cancellationToken);
    }
}
