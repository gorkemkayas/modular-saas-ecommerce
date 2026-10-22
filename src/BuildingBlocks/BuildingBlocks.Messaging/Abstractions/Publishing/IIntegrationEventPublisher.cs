namespace BuildingBlocks.Messaging.Abstractions.Publishing;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationMessage message, CancellationToken cancellationToken = default);
}

public interface IIntegrationEventPublisher<TPublisherIdentity>
    : IIntegrationEventPublisher
    where TPublisherIdentity : class
{
}