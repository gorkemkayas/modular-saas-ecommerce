namespace Shipment.Application.Exceptions;

public sealed class ShipmentAlreadyExistsForOrderException : ApplicationException
{
    public ShipmentAlreadyExistsForOrderException(Guid orderId)
        : base("An active shipment already exists for this order.")
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
