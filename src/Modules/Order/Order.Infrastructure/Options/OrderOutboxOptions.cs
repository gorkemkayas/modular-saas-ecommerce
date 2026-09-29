namespace Order.Infrastructure.Options;

public sealed class OrderOutboxOptions
{
    public const string SectionName = "Modules:Order:Outbox";
    public int BatchSize { get; set; } = 20;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ClaimDuration { get; set; } = TimeSpan.FromMinutes(2);
    public int MaximumAttempts { get; set; } = 10;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(5);
}
