namespace Shipment.Application.Exceptions;

public sealed class DuplicateShippingCarrierCodeException : ApplicationException
{
    public DuplicateShippingCarrierCodeException(string code)
        : base($"Shipping carrier code '{code}' already exists for this store.")
    {
        Code = code;
    }

    public string Code { get; }
}
