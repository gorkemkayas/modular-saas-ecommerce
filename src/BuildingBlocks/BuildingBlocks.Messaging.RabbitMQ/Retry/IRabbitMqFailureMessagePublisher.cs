using System;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public interface IRabbitMqFailureMessagePublisher<TPublisherIdentity>
    where TPublisherIdentity : class
{
    Task PublishAsync(
        string destinationExchangeName,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties sourceProperties,
        RabbitMqRetryMetadata retryMetadata,
        CancellationToken cancellationToken = default);
}
