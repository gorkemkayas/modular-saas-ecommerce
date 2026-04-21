namespace Pricing.Domain.Exceptions;

public class PricingDomainException : Exception
{
    public PricingDomainException(string message)
        : base(message)
    {
    }
}
