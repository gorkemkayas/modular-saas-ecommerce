using System.Text;
using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BuildingBlocks.Messaging.RabbitMQ.Consuming;

public sealed class RabbitMqIntegrationEventConsumer<TIdentity>(
    IRabbitMqConnectionProvider connections,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> rabbitOptions,
    IOptions<RabbitMqConsumerOptions<TIdentity>> consumerOptions,
    RabbitMqDeliveryFailureRouter<TIdentity> failures,
    ILogger<RabbitMqIntegrationEventConsumer<TIdentity>> logger) : BackgroundService
    where TIdentity : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!consumerOptions.Value.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var connection = await connections.GetConnectionAsync(RabbitMqConnectionRole.Consuming, stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await RabbitMqConsumerTopology.DeclareAsync(channel, rabbitOptions.Value.ExchangeName,
                    consumerOptions.Value, stoppingToken);
                await channel.BasicQosAsync(0, consumerOptions.Value.PrefetchCount, false, stoppingToken);
                var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken,
                    () => stopped.TrySetResult());
                consumer.UnregisteredAsync += (_, _) => { stopped.TrySetResult(); return Task.CompletedTask; };
                await channel.BasicConsumeAsync(consumerOptions.Value.QueueName, false, consumer,
                    cancellationToken: stoppingToken);
                logger.LogInformation("Consumer {Identity} started on queue {Queue}.",
                    typeof(TIdentity).Name, consumerOptions.Value.QueueName);
                while (channel.IsOpen && !stopped.Task.IsCompleted)
                {
                    await Task.WhenAny(stopped.Task, Task.Delay(TimeSpan.FromSeconds(1), stoppingToken));
                    stoppingToken.ThrowIfCancellationRequested();
                }
                stoppingToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception exception)
            {
                logger.LogError(exception, "Consumer {Identity} stopped; startup will be retried.", typeof(TIdentity).Name);
            }

            try { await Task.Delay(rabbitOptions.Value.NetworkRecoveryInterval, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs delivery,
        CancellationToken token, Action requestReconnect)
    {
        try
        {
            var message = ReadMessage(delivery);
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IIntegrationMessageReceiver<TIdentity>>()
                .ReceiveAsync(message, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Delivery {MessageId} could not be persisted by {Identity}.",
                delivery.BasicProperties.MessageId, typeof(TIdentity).Name);
            try { await failures.RouteAsync(delivery, exception is InvalidDataException, token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
            catch (Exception routingException)
            {
                logger.LogError(routingException, "Failure routing failed; original delivery will be requeued.");
                // Do not ACK an unconfirmed copy. Delay avoids a tight redelivery loop during an outage.
                try
                {
                    await Task.Delay(rabbitOptions.Value.NetworkRecoveryInterval, token);
                    await channel.BasicNackAsync(delivery.DeliveryTag, false, true, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { }
                catch (Exception nackException)
                {
                    logger.LogWarning(nackException, "Requeue failed; closing the channel for broker recovery.");
                    requestReconnect();
                }
                return;
            }
        }

        // ACK errors never enter failure routing after persistence or confirmed forwarding.
        try { await channel.BasicAckAsync(delivery.DeliveryTag, false, token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "ACK failed for {MessageId}; closing the channel for broker recovery.",
                delivery.BasicProperties.MessageId);
            requestReconnect();
        }
    }

    private IntegrationMessage ReadMessage(BasicDeliverEventArgs delivery)
    {
        var properties = delivery.BasicProperties;
        if (!Guid.TryParseExact(properties.MessageId, "D", out var id) || id == Guid.Empty ||
            string.IsNullOrWhiteSpace(properties.Type) || properties.Type.Length > 200 ||
            !string.Equals(properties.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(properties.ContentEncoding, "utf-8", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid integration message envelope.");

        string key = RabbitMqRetryMetadataCodec.TryRead(properties.Headers, out var metadata) && metadata is not null
            ? metadata.OriginalRoutingKey : delivery.RoutingKey;
        if (!consumerOptions.Value.RoutingKeys.Contains(key, StringComparer.Ordinal) || properties.Type != key)
            throw new InvalidDataException("Message type and routing key are not supported by this consumer.");
        string payload = Encoding.UTF8.GetString(delivery.Body.Span);
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (properties.Timestamp.UnixTime <= 0) throw new InvalidDataException("Message timestamp is required.");
            var occurred = DateTimeOffset.FromUnixTimeSeconds(properties.Timestamp.UnixTime).UtcDateTime;
            return new(id, properties.Type, key, payload, occurred);
        }
        catch (JsonException exception) { throw new InvalidDataException("Invalid message JSON.", exception); }
        catch (ArgumentOutOfRangeException exception) { throw new InvalidDataException("Invalid message timestamp.", exception); }
    }
}
