using Inventory.Domain.Enums;

namespace Inventory.Application.InventoryItems.DTOs;

public sealed record StockMovementDto(
    Guid Id,
    StockMovementType Type,
    int OnHandDelta,
    int ReservedDelta,
    int ResultingOnHandQuantity,
    int ResultingReservedQuantity,
    string Reason,
    string? Reference,
    DateTime CreatedAtUtc);
