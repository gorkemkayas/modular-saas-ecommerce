namespace Shipment.Application.Exceptions;

public sealed class ShipmentDispatchNotAllowedException : ApplicationException
{
    public ShipmentDispatchNotAllowedException(string message)
        : base(message)
    {
    }
}
