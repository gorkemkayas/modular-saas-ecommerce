namespace Notification.Application.Exceptions;

public sealed class NotificationTemplateAlreadyExistsException : ApplicationException
{
    public NotificationTemplateAlreadyExistsException(string message)
        : base(message)
    {
    }
}
