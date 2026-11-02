using BuildingBlocks.Messaging.RabbitMQ.Retry;

namespace BuildingBlocks.Messaging.RabbitMQ.Consuming;

public sealed class RabbitMqConsumerOptions<TIdentity> where TIdentity : class
{
    public bool Enabled { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public ushort PrefetchCount { get; set; } = 10;
    public RabbitMqRetryOptions Retry { get; set; } = new();
    public string[] RoutingKeys { get; set; } = [];
}
