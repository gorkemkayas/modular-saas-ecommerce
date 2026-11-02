using System.Text.Json;
using BuildingBlocks.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Infrastructure.Persistence;
using Shipment.Contracts.IntegrationEvents;

namespace Order.Infrastructure.Messaging.Shipment;

public sealed class ShipmentFulfillmentInboxHandler(
    OrderDbContext dbContext,
    ILogger<ShipmentFulfillmentInboxHandler> logger)
    : IModuleInboxMessageHandler<OrderFulfillmentConsumerIdentity>
{
    public string EventType => ShipmentFulfillmentChangedIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken)
    {
        ShipmentFulfillmentChangedIntegrationEvent shipment;
        try
        {
            shipment = JsonSerializer.Deserialize<ShipmentFulfillmentChangedIntegrationEvent>(
                message.Payload, JsonSerializerOptions.Web)
                ?? throw new InvalidDataException("Shipment fulfillment payload is required.");
        }
        catch (JsonException exception) { throw new InvalidDataException("Invalid shipment fulfillment payload.", exception); }

        if (message.EventType != EventType || message.RoutingKey != EventType || shipment.Id != message.Id ||
            shipment.StoreId == Guid.Empty || shipment.OrderId == Guid.Empty || shipment.ShipmentId == Guid.Empty ||
            shipment.FulfillmentRevision <= 0 || shipment.OccurredOnUtc.Kind != DateTimeKind.Utc ||
            string.IsNullOrWhiteSpace(shipment.ShipmentNumber) || !Enum.IsDefined(shipment.Status))
            throw new InvalidDataException("Shipment fulfillment identity, revision or status is invalid.");

        var order = await dbContext.Orders.SingleOrDefaultAsync(
            x => x.StoreId == shipment.StoreId && x.Id == shipment.OrderId, cancellationToken);
        // Never look up or modify another tenant's order. Bounded retry also tolerates deployment timing.
        if (order is null) throw new InvalidOperationException("The shipment's order is not available in this store.");

        bool applied;
        try
        {
            applied = order.ApplyShipmentFulfillment(shipment.ShipmentId, shipment.ShipmentNumber,
                shipment.Status switch
                {
                    ShipmentFulfillmentStatus.Draft => ShipmentFulfillmentState.Draft,
                    ShipmentFulfillmentStatus.ReadyForDispatch => ShipmentFulfillmentState.ReadyForDispatch,
                    ShipmentFulfillmentStatus.Shipped => ShipmentFulfillmentState.Shipped,
                    ShipmentFulfillmentStatus.Delivered => ShipmentFulfillmentState.Delivered,
                    ShipmentFulfillmentStatus.DeliveryException => ShipmentFulfillmentState.DeliveryException,
                    ShipmentFulfillmentStatus.Cancelled => ShipmentFulfillmentState.Cancelled,
                    _ => throw new InvalidDataException("Unsupported shipment status.")
                }, shipment.FulfillmentRevision);
        }
        catch (OrderDomainException exception)
        {
            throw new InvalidDataException("Shipment fulfillment conflicts with the order's business state.", exception);
        }
        logger.LogInformation("Shipment projection {Applied} for order {OrderId}, revision {Revision}, message {MessageId}.",
            applied ? "applied" : "skipped", shipment.OrderId, shipment.FulfillmentRevision, message.Id);
        // The Inbox processor owns SaveChanges and the transaction, including Inbox Processed.
    }
}
