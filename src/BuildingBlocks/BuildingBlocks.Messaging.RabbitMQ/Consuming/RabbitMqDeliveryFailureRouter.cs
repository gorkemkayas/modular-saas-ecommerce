using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;

namespace BuildingBlocks.Messaging.RabbitMQ.Consuming;

public sealed class RabbitMqDeliveryFailureRouter<TIdentity>(
    IRabbitMqFailureMessagePublisher<TIdentity> publisher,
    IOptions<RabbitMqConsumerOptions<TIdentity>> options,
    TimeProvider timeProvider) where TIdentity : class
{
    public Task RouteAsync(BasicDeliverEventArgs delivery, bool permanent,
        CancellationToken cancellationToken)
    {
        RabbitMqRetryMetadata metadata;
        try
        {
            metadata = RabbitMqRetryMetadataCodec.TryRead(delivery.BasicProperties.Headers, out var existing)
                && existing is not null ? existing : InitialMetadata(delivery);
        }
        catch (InvalidDataException)
        {
            permanent = true;
            metadata = InitialMetadata(delivery);
        }

        if (!permanent)
            metadata = metadata with { RetryAttempt = metadata.RetryAttempt == int.MaxValue
                ? int.MaxValue : metadata.RetryAttempt + 1 };
        string destination = !permanent && metadata.RetryAttempt <= options.Value.Retry.Delays.Length
            ? RabbitMqConsumerTopology.RetryExchange(options.Value.QueueName)
            : RabbitMqConsumerTopology.DeadLetterExchange(options.Value.QueueName);
        return publisher.PublishAsync(destination, string.Empty, delivery.Body,
            delivery.BasicProperties, metadata, cancellationToken);
    }

    private RabbitMqRetryMetadata InitialMetadata(BasicDeliverEventArgs delivery) =>
        new(0, delivery.Exchange, delivery.RoutingKey, timeProvider.GetUtcNow().UtcDateTime);
}
