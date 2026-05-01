namespace Shipment.Application.Exceptions;

public sealed class ShipmentCancellationNotAllowedException : ApplicationException
{
    public ShipmentCancellationNotAllowedException(string message)
        : base(message)
    {
    }
}
