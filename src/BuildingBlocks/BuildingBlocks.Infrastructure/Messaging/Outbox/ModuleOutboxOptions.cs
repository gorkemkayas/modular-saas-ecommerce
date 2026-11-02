namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class ModuleOutboxOptions<TIdentity> where TIdentity : class
{
    public int BatchSize { get; set; } = 20;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ClaimDuration { get; set; } = TimeSpan.FromMinutes(2);
    public int MaximumAttempts { get; set; } = 10;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    public double RetryJitterFactor { get; set; } = 0.2;
}
