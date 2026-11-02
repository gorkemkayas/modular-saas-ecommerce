namespace BuildingBlocks.Messaging.Abstractions.Publishing;

public interface IIntegrationMessageReceiver<TConsumerIdentity>
    where TConsumerIdentity : class
{
    Task ReceiveAsync(IntegrationMessage message, CancellationToken cancellationToken = default);
}
