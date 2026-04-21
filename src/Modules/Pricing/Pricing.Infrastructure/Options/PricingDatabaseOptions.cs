namespace Pricing.Infrastructure.Options;

public sealed class PricingDatabaseOptions
{
    public const string SectionName = "Modules:Pricing:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
