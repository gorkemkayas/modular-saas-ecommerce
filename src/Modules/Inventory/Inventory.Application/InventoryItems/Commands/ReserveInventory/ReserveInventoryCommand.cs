using MediatR;

namespace Inventory.Application.InventoryItems.Commands.ReserveInventory;

public sealed record ReserveInventoryCommand(
    Guid StoreId,
    Guid OrderId,
    string ReservationReference,
    IReadOnlyCollection<ReserveInventoryItemInput> Items) : IRequest;

public sealed record ReserveInventoryItemInput(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
