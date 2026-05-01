namespace Notification.Infrastructure.Options;

public sealed class NotificationEmailOptions
{
    public const string SectionName = "Modules:Notification:Email";

    public string Provider { get; set; } = "Mock";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.resend.com";
    public string WebhookSecret { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string? ReplyTo { get; set; }
}
