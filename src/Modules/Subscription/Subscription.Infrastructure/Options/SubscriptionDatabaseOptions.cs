namespace Subscription.Infrastructure.Options;

public sealed class SubscriptionDatabaseOptions
{
    public const string SectionName = "Modules:Subscription:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
