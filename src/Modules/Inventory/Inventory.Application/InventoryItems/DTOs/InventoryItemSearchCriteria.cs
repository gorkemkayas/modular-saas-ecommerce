namespace Inventory.Application.InventoryItems.DTOs;

public sealed record InventoryItemSearchCriteria(
    Guid StoreId,
    Guid? ProductId,
    Guid? ProductVariantId,
    bool OnlyLowStock,
    string? SearchTerm,
    int PageNumber,
    int PageSize);
