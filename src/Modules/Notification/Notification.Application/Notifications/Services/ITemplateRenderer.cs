namespace Notification.Application.Notifications.Services;

public interface ITemplateRenderer
{
    string Render(
        string template,
        IReadOnlyDictionary<string, string?> values);
}
