namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public interface IModuleInboxMessageHandler<TIdentity> where TIdentity : class
{
    string EventType { get; }
    Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken = default);
}
