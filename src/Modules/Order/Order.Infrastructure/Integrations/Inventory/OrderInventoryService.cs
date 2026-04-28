using Inventory.Contracts;
using Order.Application.Exceptions;
using Order.Application.Integrations;

namespace Order.Infrastructure.Integrations.Inventory;

public sealed class OrderInventoryService : IOrderInventoryService
{
    private readonly IInventoryModuleApi _inventoryModuleApi;

    public OrderInventoryService(IInventoryModuleApi inventoryModuleApi)
    {
        _inventoryModuleApi = inventoryModuleApi;
    }

    public async Task EnsureAvailabilityAsync(
        Guid storeId,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default)
    {
        var result = await _inventoryModuleApi.CheckAvailabilityAsync(
            new CheckInventoryAvailabilityRequest(
                storeId,
                items.Select(x => new InventoryAvailabilityItemRequest(
                    x.ProductId,
                    x.ProductVariantId,
                    x.Quantity)).ToArray()),
            cancellationToken);

        if (result.IsAvailable)
            return;

        var unavailableItem = result.Items.FirstOrDefault(x => !x.IsAvailable);

        if (unavailableItem is not null)
        {
            throw new OrderInventoryUnavailableException(
                $"Insufficient stock for product '{unavailableItem.ProductId}' and variant '{unavailableItem.ProductVariantId}'. Requested {unavailableItem.RequestedQuantity}, available {unavailableItem.AvailableQuantity}.");
        }

        throw new OrderInventoryUnavailableException("Insufficient stock for one or more order items.");
    }

    public Task ReserveAsync(
        Guid storeId,
        Guid orderId,
        string reservationReference,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default)
    {
        return _inventoryModuleApi.ReserveAsync(
            new ReserveInventoryRequest(
                storeId,
                orderId,
                reservationReference,
                items.Select(x => new InventoryReservationItemRequest(
                    x.ProductId,
                    x.ProductVariantId,
                    x.Quantity)).ToArray()),
            cancellationToken);
    }

    public Task ReleaseReservationAsync(
        Guid storeId,
        string reservationReference,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return _inventoryModuleApi.ReleaseReservationAsync(
            new ReleaseInventoryReservationRequest(storeId, reservationReference, reason),
            cancellationToken);
    }
}
