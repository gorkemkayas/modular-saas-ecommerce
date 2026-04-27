using Order.Application.Integrations;

namespace ECommerce.API.Integrations.Order;

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
