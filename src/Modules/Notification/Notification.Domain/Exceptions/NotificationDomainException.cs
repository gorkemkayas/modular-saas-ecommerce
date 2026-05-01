namespace Notification.Domain.Exceptions;

public sealed class NotificationDomainException : Exception
{
    public NotificationDomainException(string message)
        : base(message)
    {
    }
}
