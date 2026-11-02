namespace Notification.Infrastructure.Options;

public sealed class NotificationDispatchOptions
{
    public const string SectionName = "Modules:Notification:DispatchProcessor";

    public bool Enabled { get; set; }
    public int BatchSize { get; set; } = 20;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ClaimDuration { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaximumAttempts { get; set; } = 10;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(10);
    public double RetryJitterFactor { get; set; } = 0.2;

    // Resend retains idempotency keys for 24 hours. Stop automatic retries earlier.
    public TimeSpan MaximumDeliveryAge { get; set; } = TimeSpan.FromHours(23);
}
