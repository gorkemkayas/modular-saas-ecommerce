namespace Shipment.Application.Exceptions;

public sealed class ShipmentValidationException : ApplicationException
{
    public ShipmentValidationException(string message)
        : base(message)
    {
    }
}
