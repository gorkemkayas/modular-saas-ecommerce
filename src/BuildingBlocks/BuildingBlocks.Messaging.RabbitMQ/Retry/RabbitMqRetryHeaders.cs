using System;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public static class RabbitMqRetryHeaders
{
    public const string RetryAttempt = "x-ecommerce-retry-attempt";
    public const string OriginalExchange = "x-ecommerce-original-exchange";
    public const string OriginalRoutingKey = "x-ecommerce-original-routing-key";
    public const string FirstFailedAtUnixTime = "x-ecommerce-first-failed-at";
}
