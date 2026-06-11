namespace Subscription.Domain.Exceptions;

public class SubscriptionDomainException : Exception
{
    public SubscriptionDomainException(string message)
        : base(message)
    {
    }
}
