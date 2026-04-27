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
}
