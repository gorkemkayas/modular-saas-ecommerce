namespace Shipment.Contracts.IntegrationEvents;

public enum ShipmentFulfillmentStatus
{
    Draft = 0,
    ReadyForDispatch = 1,
    Shipped = 2,
    Delivered = 3,
    DeliveryException = 4,
    Cancelled = 5
}
