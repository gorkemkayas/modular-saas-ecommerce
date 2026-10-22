namespace Notification.Infrastructure.Options;

public sealed class NotificationInboxOptions
{
    public const string SectionName =
        "Modules:Notification:InboxProcessor";

    public bool Enabled { get; set; }

    public int BatchSize { get; set; } = 20;

    public TimeSpan PollingInterval { get; set; }
        = TimeSpan.FromSeconds(5);

    public TimeSpan ClaimDuration { get; set; }
        = TimeSpan.FromMinutes(2);

    public int MaximumAttempts { get; set; } = 10;

    public TimeSpan InitialRetryDelay { get; set; }
        = TimeSpan.FromSeconds(5);

    public TimeSpan MaximumRetryDelay { get; set; }
        = TimeSpan.FromMinutes(5);

    public double RetryJitterFactor { get; set; } = 0.20;
}