namespace Notification.Infrastructure.Persistence.Inbox;

public interface IInboxMessageWriter
{
    Task<InboxMessageWriteResult> WriteAsync(
        InboxMessage message,
        CancellationToken cancellationToken = default);
}