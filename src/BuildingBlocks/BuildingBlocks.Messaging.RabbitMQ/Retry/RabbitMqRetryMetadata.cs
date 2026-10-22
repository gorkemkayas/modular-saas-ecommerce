using System;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public sealed record RabbitMqRetryMetadata(
    int RetryAttempt,
    string OriginalExchange,
    string OriginalRoutingKey,
    DateTime FirstFailedOnUtc);