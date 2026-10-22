using BuildingBlocks.Messaging.RabbitMQ.Retry;

namespace Notification.Infrastructure.Options;

public sealed class NotificationIntegrationEventConsumerOptions
{
    public const string SectionName = "Modules:Notification:IntegrationEventConsumer";
    public bool Enabled { get; set; }
    public string QueueName { get; set; } = "notification.integration-events";
    public ushort PrefetchCount { get; set; } = 10;
    public RabbitMqRetryOptions Retry { get; set; } = new();
}
