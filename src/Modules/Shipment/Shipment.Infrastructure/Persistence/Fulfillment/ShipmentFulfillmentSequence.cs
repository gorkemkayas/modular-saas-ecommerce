namespace Shipment.Infrastructure.Persistence.Fulfillment;

public sealed class ShipmentFulfillmentSequence
{
    private ShipmentFulfillmentSequence() { }

    public Guid StoreId { get; private set; }
    public Guid OrderId { get; private set; }
    public long LastRevision { get; private set; }
}
