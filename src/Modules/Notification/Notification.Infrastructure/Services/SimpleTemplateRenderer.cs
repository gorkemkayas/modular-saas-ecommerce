using System.Text.RegularExpressions;
using Notification.Application.Notifications.Services;

namespace Notification.Infrastructure.Services;

public sealed class SimpleTemplateRenderer : ITemplateRenderer
{
    private static readonly Regex TokenRegex = new(@"\{\{\s*(?<key>[A-Za-z0-9_]+)\s*\}\}", RegexOptions.Compiled);

    public string Render(string template, IReadOnlyDictionary<string, string?> values)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        return TokenRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return values.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
        });
    }
}
