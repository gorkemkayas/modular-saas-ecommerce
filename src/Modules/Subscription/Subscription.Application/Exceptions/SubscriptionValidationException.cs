namespace Subscription.Application.Exceptions;

public sealed class SubscriptionValidationException : ApplicationException
{
    public SubscriptionValidationException(string message)
        : base(message)
    {
    }
}
