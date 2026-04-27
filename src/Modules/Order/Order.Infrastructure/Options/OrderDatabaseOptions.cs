namespace Order.Infrastructure.Options;

public sealed class OrderDatabaseOptions
{
    public const string SectionName = "Modules:Order:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
