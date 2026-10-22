using System;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public sealed class RabbitMqRetryOptions
{
    public TimeSpan[] Delays { get; set; } =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10)
    ];
}
