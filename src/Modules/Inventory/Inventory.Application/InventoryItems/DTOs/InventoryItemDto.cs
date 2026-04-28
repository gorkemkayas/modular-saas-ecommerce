namespace Inventory.Application.InventoryItems.DTOs;

public sealed record InventoryItemDto(
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
    int Version,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<InventoryReservationDto> Reservations,
    IReadOnlyCollection<StockMovementDto> RecentMovements);
