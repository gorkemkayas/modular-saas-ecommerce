namespace Shipment.Application.Exceptions;

public sealed class ShipmentCreationNotAllowedException : ApplicationException
{
    public ShipmentCreationNotAllowedException(string message)
        : base(message)
    {
    }
}
