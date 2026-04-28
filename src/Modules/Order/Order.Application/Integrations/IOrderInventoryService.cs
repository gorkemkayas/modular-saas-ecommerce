namespace Order.Application.Integrations;

public interface IOrderInventoryService
{
    Task EnsureAvailabilityAsync(
        Guid storeId,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default);

    Task ReserveAsync(
        Guid storeId,
        Guid orderId,
        string reservationReference,
        IReadOnlyCollection<OrderInventoryItemRequest> items,
        CancellationToken cancellationToken = default);

    Task ReleaseReservationAsync(
        Guid storeId,
        string reservationReference,
        string reason,
        CancellationToken cancellationToken = default);
}
