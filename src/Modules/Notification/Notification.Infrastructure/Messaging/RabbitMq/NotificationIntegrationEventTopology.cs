using Order.Contracts.IntegrationEvents;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using RabbitMQ.Client;

namespace Notification.Infrastructure.Messaging.RabbitMq;

internal static class NotificationIntegrationEventTopology
{
    private static readonly string[] RoutingKeys =
    [
        OrderCancelledIntegrationEvent.EventType
    ];

    public static async Task DeclareAsync(
        IChannel channel,
        string exchangeName,
        string queueName,
        RabbitMqRetryOptions retryOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(retryOptions);

        if (string.IsNullOrWhiteSpace(exchangeName))
        {
            throw new ArgumentException(
                "RabbitMQ exchange name is required.",
                nameof(exchangeName));
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException(
                "Notification queue name is required.",
                nameof(queueName));
        }

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        foreach (string routingKey in RoutingKeys)
        {
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey,
                arguments: null,
                noWait: false,
                cancellationToken: cancellationToken);
        }

        await DeclareRetryTopologyAsync(
            channel,
            queueName,
            retryOptions,
            cancellationToken);

        await DeclareDeadLetterTopologyAsync(
            channel,
            queueName,
            cancellationToken);
    }

    public static string GetRetryExchangeName(string queueName)
    {
        return $"{queueName}.retry";
    }

    public static string GetRetryQueueName(string queueName, int retryAttempt)
    {
        return $"{queueName}.retry.{retryAttempt}";
    }

    public static string GetDeadLetterExchangeName(string queueName)
    {
        return $"{queueName}.dead-letter";
    }

    public static string GetDeadLetterQueueName(string queueName)
    {
        return $"{queueName}.dead-letter.queue";
    }

    private static async Task DeclareRetryTopologyAsync(
        IChannel channel,
        string mainQueueName,
        RabbitMqRetryOptions retryOptions,
        CancellationToken cancellationToken)
    {
        string retryExchangeName = GetRetryExchangeName(mainQueueName);

        await channel.ExchangeDeclareAsync(
            exchange: retryExchangeName,
            type: ExchangeType.Headers,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        for(int index = 0; index < retryOptions.Delays.Length; index++)
        {
            int retryAttempt = index + 1;

            TimeSpan retryDelay = retryOptions.Delays[index];

            string retryQueueName = GetRetryQueueName(mainQueueName, retryAttempt);

            long retryDelayMilliseconds = checked((long)retryDelay.TotalMilliseconds);

            var queueArguments = 
                new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = retryDelayMilliseconds,
                    ["x-dead-letter-exchange"] = string.Empty,
                    ["x-dead-letter-routing-key"] = mainQueueName
                };

            await channel.QueueDeclareAsync(
                queue: retryQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments,
                passive: false,
                noWait: false,
                cancellationToken: cancellationToken);
            
            var bindingArguments =
                new Dictionary<string, object?>
                {
                    ["x-match"] = "all",
                    [RabbitMqRetryHeaders.RetryAttempt] = retryAttempt
                };
            
            await channel.QueueBindAsync(
                queue: retryQueueName,
                exchange: retryExchangeName,
                routingKey: string.Empty,
                arguments: bindingArguments,
                noWait: false,
                cancellationToken: cancellationToken);
        }
    }

    private static async Task DeclareDeadLetterTopologyAsync(
        IChannel channel,
        string mainQueueName,
        CancellationToken cancellationToken)
    {
        string deadLetterExchangeName =
            GetDeadLetterExchangeName(mainQueueName);

        string deadLetterQueueName =
            GetDeadLetterQueueName(mainQueueName);

        await channel.ExchangeDeclareAsync(
            exchange: deadLetterExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: string.Empty,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);
    }
}