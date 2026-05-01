namespace Shipment.Infrastructure.Options;

public sealed class ShipmentDatabaseOptions
{
    public const string SectionName = "Modules:Shipment:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
