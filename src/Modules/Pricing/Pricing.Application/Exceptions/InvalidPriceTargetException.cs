namespace Pricing.Application.Exceptions;

public sealed class InvalidPriceTargetException : ApplicationException
{
    public InvalidPriceTargetException(string message)
        : base(message)
    {
    }
}
