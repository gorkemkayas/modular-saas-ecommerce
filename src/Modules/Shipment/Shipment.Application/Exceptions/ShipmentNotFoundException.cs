namespace Shipment.Application.Exceptions;

public sealed class ShipmentNotFoundException : ApplicationException
{
    public ShipmentNotFoundException(Guid shipmentId)
        : base("Shipment was not found.")
    {
        ShipmentId = shipmentId;
    }

    public Guid ShipmentId { get; }
}
