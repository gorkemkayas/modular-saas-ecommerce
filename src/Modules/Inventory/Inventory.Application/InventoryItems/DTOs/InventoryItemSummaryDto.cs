namespace Inventory.Application.InventoryItems.DTOs;

public sealed record InventoryItemSummaryDto(
    Guid Id,
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    string Sku,
    string DisplayName,
    int OnHandQuantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int? ReorderThreshold,
    bool IsLowStock,
    DateTime UpdatedAtUtc);
