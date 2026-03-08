namespace Store.Infrastructure.Options
{
    public sealed class StoreDatabaseOptions
    {
        public const string SectionName = "Modules:Store:Database";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
