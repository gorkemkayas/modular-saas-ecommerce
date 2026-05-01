namespace Shipment.Domain.Exceptions;

public class ShipmentDomainException : Exception
{
    public ShipmentDomainException(string message)
        : base(message)
    {
    }
}
