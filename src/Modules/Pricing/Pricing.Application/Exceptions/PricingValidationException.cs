namespace Pricing.Application.Exceptions;

public sealed class PricingValidationException : ApplicationException
{
    public PricingValidationException(string message)
        : base(message)
    {
    }
}
