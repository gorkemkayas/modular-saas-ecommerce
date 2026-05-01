namespace Notification.Application.Exceptions;

public sealed class NotificationTemplateValidationException : ApplicationException
{
    public NotificationTemplateValidationException(string message)
        : base(message)
    {
    }
}
