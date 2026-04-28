using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.SearchInventoryItems;

public sealed record SearchInventoryItemsQuery(
    Guid StoreId,
    Guid? ProductId,
    Guid? ProductVariantId,
    bool OnlyLowStock,
    string? SearchTerm,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<InventoryItemSummaryDto>>;
