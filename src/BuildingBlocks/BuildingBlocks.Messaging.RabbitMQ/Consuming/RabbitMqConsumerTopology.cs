using BuildingBlocks.Messaging.RabbitMQ.Retry;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Consuming;

public static class RabbitMqConsumerTopology
{
    public static string RetryExchange(string queue) => $"{queue}.retry";
    public static string RetryQueue(string queue, int attempt) => $"{queue}.retry.{attempt}";
    public static string DeadLetterExchange(string queue) => $"{queue}.dead-letter";
    public static string DeadLetterQueue(string queue) => $"{queue}.dead-letter.queue";

    public static async Task DeclareAsync<TIdentity>(IChannel channel, string exchange,
        RabbitMqConsumerOptions<TIdentity> options, CancellationToken cancellationToken = default)
        where TIdentity : class
    {
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true,
            autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(options.QueueName, durable: true, exclusive: false,
            autoDelete: false, cancellationToken: cancellationToken);
        foreach (string key in options.RoutingKeys)
            await channel.QueueBindAsync(options.QueueName, exchange, key, cancellationToken: cancellationToken);

        string retryExchange = RetryExchange(options.QueueName);
        await channel.ExchangeDeclareAsync(retryExchange, ExchangeType.Headers,
            durable: true, autoDelete: false, cancellationToken: cancellationToken);
        for (int index = 0; index < options.Retry.Delays.Length; index++)
        {
            string queue = RetryQueue(options.QueueName, index + 1);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = checked((long)options.Retry.Delays[index].TotalMilliseconds),
                    ["x-dead-letter-exchange"] = string.Empty,
                    ["x-dead-letter-routing-key"] = options.QueueName
                }, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queue, retryExchange, string.Empty,
                arguments: new Dictionary<string, object?>
                {
                    ["x-match"] = "all", [RabbitMqRetryHeaders.RetryAttempt] = index + 1
                }, cancellationToken: cancellationToken);
        }

        await channel.ExchangeDeclareAsync(DeadLetterExchange(options.QueueName), ExchangeType.Fanout,
            durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(DeadLetterQueue(options.QueueName), durable: true, exclusive: false,
            autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(DeadLetterQueue(options.QueueName), DeadLetterExchange(options.QueueName),
            string.Empty, cancellationToken: cancellationToken);
    }
}
