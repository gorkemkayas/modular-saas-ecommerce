namespace Notification.Application.Exceptions;

public sealed class NotificationTemplateNotFoundException : ApplicationException
{
    public NotificationTemplateNotFoundException(Guid templateId)
        : base($"Notification template '{templateId}' was not found.")
    {
    }
}
