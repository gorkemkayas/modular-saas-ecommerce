using MediatR;

namespace Inventory.Application.InventoryItems.Commands.CreateInventoryItem;

public sealed record CreateInventoryItemCommand(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    int InitialOnHandQuantity,
    int? ReorderThreshold) : IRequest<Guid>;
