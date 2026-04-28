using Order.Application.Integrations;

namespace Order.Infrastructure.Integrations.Inventory;

public sealed class NoOpOrderInventoryService : IOrderInventoryService
{
    public Task EnsureAvailabilityAsync(
        Guid storeId,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ReserveAsync(
        Guid storeId,
        Guid orderId,
        string reservationReference,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ReleaseReservationAsync(
        Guid storeId,
        string reservationReference,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
