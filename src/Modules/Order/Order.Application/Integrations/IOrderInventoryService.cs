namespace Order.Application.Integrations;

public interface IOrderInventoryService
{
    Task EnsureAvailabilityAsync(
        Guid storeId,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default);
}
