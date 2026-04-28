using Inventory.Application.Abstractions.Queries;
using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.SearchInventoryItems;

public sealed class SearchInventoryItemsQueryHandler : IRequestHandler<SearchInventoryItemsQuery, PagedResult<InventoryItemSummaryDto>>
{
    private readonly IInventoryReadService _inventoryReadService;

    public SearchInventoryItemsQueryHandler(IInventoryReadService inventoryReadService)
    {
        _inventoryReadService = inventoryReadService;
    }

    public Task<PagedResult<InventoryItemSummaryDto>> Handle(SearchInventoryItemsQuery query, CancellationToken cancellationToken)
    {
        return _inventoryReadService.SearchAsync(
            new InventoryItemSearchCriteria(
                query.StoreId,
                query.ProductId,
                query.ProductVariantId,
                query.OnlyLowStock,
                query.SearchTerm,
                query.PageNumber,
                query.PageSize),
            cancellationToken);
    }
}
