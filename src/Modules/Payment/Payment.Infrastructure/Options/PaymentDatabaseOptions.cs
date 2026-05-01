namespace Payment.Infrastructure.Options;

public sealed class PaymentDatabaseOptions
{
    public const string SectionName = "Modules:Payment:Database";

    public string ConnectionString { get; init; } = string.Empty;
}
