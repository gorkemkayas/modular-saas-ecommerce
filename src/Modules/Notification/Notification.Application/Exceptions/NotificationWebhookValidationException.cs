namespace Notification.Application.Exceptions;

public sealed class NotificationWebhookValidationException : ApplicationException
{
    public NotificationWebhookValidationException(string message)
        : base(message)
    {
    }
}
