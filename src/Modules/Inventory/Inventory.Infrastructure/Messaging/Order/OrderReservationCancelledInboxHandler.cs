using System.Text.Json;
using BuildingBlocks.Infrastructure.Messaging.Inbox;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Repositories;
using Order.Contracts.IntegrationEvents;

namespace Inventory.Infrastructure.Messaging.Order;

public sealed class OrderReservationCancelledInboxHandler(IInventoryItemRepository repository)
    : IModuleInboxMessageHandler<InventoryReservationConsumerIdentity>
{
    public string EventType => OrderReservationCancelledIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken)
    {
        OrderReservationCancelledIntegrationEvent cancellation;
        try
        {
            cancellation = JsonSerializer.Deserialize<OrderReservationCancelledIntegrationEvent>(
                message.Payload, JsonSerializerOptions.Web)
                ?? throw new InvalidDataException("Reservation cancellation payload is required.");
        }
        catch (JsonException exception) { throw new InvalidDataException("Invalid reservation cancellation JSON.", exception); }
        if (message.EventType != EventType || message.RoutingKey != EventType || cancellation.Id != message.Id ||
            cancellation.StoreId == Guid.Empty || cancellation.OrderId == Guid.Empty ||
            cancellation.OccurredOnUtc.Kind != DateTimeKind.Utc ||
            string.IsNullOrWhiteSpace(cancellation.ReservationReference) || cancellation.ReservationReference.Length > 100 ||
            cancellation.ReservationReference != cancellation.ReservationReference.Trim() || cancellation.CancellationReason?.Length > 500)
            throw new InvalidDataException("Invalid reservation cancellation identity or payload.");

        var items = await repository.ListByReservationReferenceAsync(
            cancellation.StoreId, cancellation.ReservationReference, cancellationToken);
        if (items.Count == 0)
            throw new InvalidOperationException("The cancelled order reservation is not available in this store.");

        // Validate the entire reservation before changing any item. Never release another order's stock,
        // and never convert confirmed stock deductions back into availability.
        foreach (var item in items)
        {
            var reservation = item.Reservations.Single(x => x.ReservationReference == cancellation.ReservationReference);
            if (item.StoreId != cancellation.StoreId || reservation.OrderId != cancellation.OrderId)
                throw new InvalidDataException("Reservation ownership does not match the cancelled order.");
            if (reservation.Status == InventoryReservationStatus.Confirmed)
                throw new InvalidDataException("A confirmed reservation cannot be released by order cancellation.");
        }
        try
        {
            foreach (var item in items)
                item.ReleaseReservation(cancellation.ReservationReference,
                    cancellation.CancellationReason ?? "Order cancelled by customer.");
        }
        catch (InventoryDomainException exception)
        { throw new InvalidDataException("Reservation cancellation violates Inventory business rules.", exception); }
        // Processor commits all stock movements, reservation changes and Inbox Processed atomically.
    }
}
