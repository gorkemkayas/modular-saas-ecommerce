namespace Inventory.Infrastructure.Options;

public sealed class InventoryDatabaseOptions
{
    public const string SectionName = "Modules:Inventory:Database";

    public string ConnectionString { get; init; } = string.Empty;
}
