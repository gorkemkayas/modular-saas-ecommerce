namespace Catalog.Infrastructure.Options
{
    public sealed class CatalogDatabaseOptions
    {
        public const string SectionName = "Modules:Catalog:Database";

        public string ConnectionString { get; set; } = string.Empty;
    }
}
