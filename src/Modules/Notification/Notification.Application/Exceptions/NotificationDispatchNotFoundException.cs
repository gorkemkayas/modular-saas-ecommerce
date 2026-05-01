namespace Notification.Application.Exceptions;

public sealed class NotificationDispatchNotFoundException : ApplicationException
{
    public NotificationDispatchNotFoundException(Guid dispatchId)
        : base($"Notification dispatch '{dispatchId}' was not found.")
    {
    }
}
