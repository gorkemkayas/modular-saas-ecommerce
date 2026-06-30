namespace Notification.Infrastructure.Options;

public sealed class NotificationBrandingOptions
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; set; } = string.Empty;
}
