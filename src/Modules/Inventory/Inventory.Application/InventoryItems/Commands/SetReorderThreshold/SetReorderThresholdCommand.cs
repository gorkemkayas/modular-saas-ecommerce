using MediatR;

namespace Inventory.Application.InventoryItems.Commands.SetReorderThreshold;

public sealed record SetReorderThresholdCommand(
    Guid StoreId,
    Guid InventoryItemId,
    int? ReorderThreshold) : IRequest;
