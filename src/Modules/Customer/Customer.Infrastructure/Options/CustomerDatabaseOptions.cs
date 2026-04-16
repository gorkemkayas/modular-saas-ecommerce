namespace Customer.Infrastructure.Options;

public sealed class CustomerDatabaseOptions
{
    public const string SectionName = "Modules:Customer:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
