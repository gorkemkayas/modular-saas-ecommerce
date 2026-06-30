using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications.Services;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Services;

/// <summary>
/// Builds email-safe, branded HTML using table-based layout and inline CSS so it
/// renders consistently across mail clients (Gmail, Outlook, Apple Mail).
/// The visual language mirrors the storefront: minimalist, monochrome, sharp
/// corners (radius 0), serif heading, sans-serif body.
/// </summary>
public sealed class BrandedEmailComposer : IEmailLayoutComposer
{
    private const string PageBackground = "#f5f5f5";
    private const string Surface = "#ffffff";
    private const string Border = "#e5e5e5";
    private const string Ink = "#0a0a0a";
    private const string Muted = "#737373";
    private const string SerifStack = "'Cormorant Garamond', Georgia, 'Times New Roman', serif";
    private const string SansStack = "'Inter', Arial, Helvetica, sans-serif";

    private readonly NotificationBrandingOptions _options;

    public BrandedEmailComposer(IOptions<NotificationBrandingOptions> options)
    {
        _options = options.Value;
    }

    public string ComposeHtml(EmailComposition composition)
    {
        var storeName = Encode(string.IsNullOrWhiteSpace(composition.StoreName) ? "Store" : composition.StoreName);
        var title = Encode(composition.Title);

        var content = new StringBuilder();

        content.Append($"""
            <tr><td style="padding:40px 40px 24px 40px;text-align:center;border-bottom:1px solid {Border};">
              <span style="font-family:{SerifStack};font-size:26px;letter-spacing:2px;color:{Ink};text-transform:uppercase;">{storeName}</span>
            </td></tr>
            """);

        content.Append($"""
            <tr><td style="padding:36px 40px 8px 40px;">
              <h1 style="margin:0;font-family:{SansStack};font-size:20px;font-weight:600;color:{Ink};">{title}</h1>
            </td></tr>
            """);

        content.Append($"""
            <tr><td style="padding:8px 40px 24px 40px;font-family:{SansStack};font-size:15px;line-height:1.65;color:{Ink};">
              {RenderBody(composition.BodyText)}
            </td></tr>
            """);

        if (composition.LineItems.Count > 0)
        {
            content.Append(RenderLineItems(composition.LineItems));
        }

        if (composition.Totals.Count > 0)
        {
            content.Append(RenderTotals(composition.Totals));
        }

        if (composition.Details.Count > 0)
        {
            content.Append(RenderDetails(composition.Details));
        }

        if (composition.CallToAction is not null)
        {
            content.Append(RenderCallToAction(composition.CallToAction));
        }

        content.Append($"""
            <tr><td style="padding:28px 40px 36px 40px;border-top:1px solid {Border};font-family:{SansStack};font-size:12px;line-height:1.6;color:{Muted};text-align:center;">
              {RenderFooter(storeName)}
            </td></tr>
            """);

        return $"""
            <!DOCTYPE html>
            <html lang="tr">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <meta name="color-scheme" content="light only" />
              <title>{title}</title>
            </head>
            <body style="margin:0;padding:0;background-color:{PageBackground};">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:{PageBackground};">
                <tr><td align="center" style="padding:24px 12px;">
                  <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="width:600px;max-width:100%;background-color:{Surface};border:1px solid {Border};">
                    {content}
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string RenderBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        return Encode(body)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n\n", "</p><p style=\"margin:0 0 14px 0;\">", StringComparison.Ordinal)
            .Replace("\n", "<br />", StringComparison.Ordinal)
            .Insert(0, "<p style=\"margin:0 0 14px 0;\">")
            + "</p>";
    }

    private string RenderLineItems(IReadOnlyCollection<EmailLineItem> items)
    {
        var rows = new StringBuilder();

        foreach (var item in items)
        {
            var name = Encode(item.Name);
            var variant = string.IsNullOrWhiteSpace(item.Variant)
                ? string.Empty
                : $"<br /><span style=\"color:{Muted};font-size:13px;\">{Encode(item.Variant!)}</span>";

            var imageCell = string.IsNullOrWhiteSpace(item.ImageUrl)
                ? string.Empty
                : $"""
                  <td width="68" style="padding:12px 12px 12px 0;border-bottom:1px solid {Border};vertical-align:top;">
                    <img src="{Encode(item.ImageUrl!)}" width="56" height="56" alt="{name}" style="display:block;width:56px;height:56px;object-fit:cover;border:1px solid {Border};" />
                  </td>
                  """;

            rows.Append($"""
                <tr>
                  {imageCell}
                  <td style="padding:12px 0;border-bottom:1px solid {Border};font-family:{SansStack};font-size:14px;color:{Ink};vertical-align:top;">
                    {name}{variant}
                    <br /><span style="color:{Muted};font-size:13px;">Qty: {item.Quantity}</span>
                  </td>
                  <td style="padding:12px 0;border-bottom:1px solid {Border};font-family:{SansStack};font-size:14px;color:{Ink};text-align:right;white-space:nowrap;vertical-align:top;">{Encode(item.Amount)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:0 40px;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{rows}</table>
            </td></tr>
            """;
    }

    private string RenderTotals(IReadOnlyCollection<EmailDetailRow> totals)
    {
        var rows = new StringBuilder();
        var entries = totals.ToArray();

        for (var i = 0; i < entries.Length; i++)
        {
            var isLast = i == entries.Length - 1;
            var weight = isLast ? "600" : "400";
            var color = isLast ? Ink : Muted;
            var size = isLast ? "16px" : "14px";

            rows.Append($"""
                <tr>
                  <td style="padding:6px 0;font-family:{SansStack};font-size:{size};font-weight:{weight};color:{color};">{Encode(entries[i].Label)}</td>
                  <td style="padding:6px 0;font-family:{SansStack};font-size:{size};font-weight:{weight};color:{color};text-align:right;white-space:nowrap;">{Encode(entries[i].Value)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:16px 40px 8px 40px;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{rows}</table>
            </td></tr>
            """;
    }

    private string RenderDetails(IReadOnlyCollection<EmailDetailRow> details)
    {
        var rows = new StringBuilder();

        foreach (var row in details)
        {
            rows.Append($"""
                <tr>
                  <td style="padding:8px 0;font-family:{SansStack};font-size:13px;color:{Muted};">{Encode(row.Label)}</td>
                  <td style="padding:8px 0;font-family:{SansStack};font-size:14px;color:{Ink};text-align:right;">{Encode(row.Value)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:8px 40px 16px 40px;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border:1px solid {Border};border-collapse:separate;">
                <tr><td style="padding:8px 16px;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{rows}</table>
                </td></tr>
              </table>
            </td></tr>
            """;
    }

    private string RenderCallToAction(EmailCallToAction cta)
    {
        var url = Encode(ResolveUrl(cta.Url));
        var text = Encode(cta.Text);

        // Table-based "bulletproof" button for Outlook compatibility.
        return $"""
            <tr><td style="padding:8px 40px 32px 40px;">
              <table role="presentation" cellpadding="0" cellspacing="0">
                <tr><td style="background-color:{Ink};">
                  <a href="{url}" style="display:inline-block;padding:14px 32px;font-family:{SansStack};font-size:13px;font-weight:600;letter-spacing:1px;text-transform:uppercase;color:#ffffff;text-decoration:none;">{text}</a>
                </td></tr>
              </table>
            </td></tr>
            """;
    }

    private string RenderFooter(string encodedStoreName)
    {
        var baseUrl = _options.BaseUrl?.Trim();

        var link = string.IsNullOrWhiteSpace(baseUrl)
            ? string.Empty
            : $"""<br /><a href="{Encode(baseUrl!)}" style="color:{Muted};text-decoration:underline;">{Encode(StripScheme(baseUrl!))}</a>""";

        return $"""
            {encodedStoreName}
            <br />You are receiving this email because of an action on your account.
            {link}
            """;
    }

    private string ResolveUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "#";

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var baseUrl = _options.BaseUrl?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(baseUrl))
            return url;

        return $"{baseUrl}/{url.TrimStart('/')}";
    }

    private static string StripScheme(string url)
    {
        return url
            .Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');
    }

    private static string Encode(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
