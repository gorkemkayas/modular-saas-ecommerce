namespace Shipment.Application.Exceptions;

public sealed class UnauthorizedShipmentAccessException : ApplicationException
{
    public UnauthorizedShipmentAccessException()
        : base("The current user is not allowed to access this shipment.")
    {
    }
}
