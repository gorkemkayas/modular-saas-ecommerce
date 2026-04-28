using MediatR;

namespace Inventory.Application.InventoryItems.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid StoreId,
    Guid InventoryItemId,
    int NewOnHandQuantity,
    string Reason,
    string? Reference) : IRequest;
