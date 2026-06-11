namespace Shipment.Application.Exceptions;

public sealed class ShippingCarrierNotFoundException : ApplicationException
{
    public ShippingCarrierNotFoundException(Guid carrierId)
        : base("Shipping carrier was not found.")
    {
        CarrierId = carrierId;
    }

    public Guid CarrierId { get; }
}
