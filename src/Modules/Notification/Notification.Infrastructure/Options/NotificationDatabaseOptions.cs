namespace Notification.Infrastructure.Options;

public sealed class NotificationDatabaseOptions
{
    public const string SectionName = "Modules:Notification:Database";

    public string ConnectionString { get; set; } = string.Empty;
}
