using MediatR;

namespace Inventory.Application.InventoryItems.Commands.AddStock;

public sealed record AddStockCommand(
    Guid StoreId,
    Guid InventoryItemId,
    int Quantity,
    string Reason,
    string? Reference) : IRequest;
