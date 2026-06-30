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
/// corners (radius 0), serif display type, sans-serif body, generous whitespace.
/// </summary>
public sealed class BrandedEmailComposer : IEmailLayoutComposer
{
    private const string PageBackground = "#f4f4f5";
    private const string Surface = "#ffffff";
    private const string Border = "#e7e7e7";
    private const string Hairline = "#eeeeee";
    private const string Ink = "#0a0a0a";
    private const string Body = "#454545";
    private const string Muted = "#8a8a8a";
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

        // Brand header
        content.Append($"""
            <tr><td style="padding:48px 48px 0 48px;text-align:center;">
              <span style="font-family:{SerifStack};font-size:28px;font-weight:600;letter-spacing:4px;color:{Ink};text-transform:uppercase;">{storeName}</span>
              <div style="width:40px;height:1px;background-color:{Ink};margin:20px auto 0 auto;line-height:1px;font-size:0;">&nbsp;</div>
            </td></tr>
            """);

        // Eyebrow + title
        var eyebrow = string.IsNullOrWhiteSpace(composition.Eyebrow)
            ? string.Empty
            : $"""<div style="font-family:{SansStack};font-size:11px;font-weight:600;letter-spacing:3px;text-transform:uppercase;color:{Muted};margin-bottom:12px;">{Encode(composition.Eyebrow!)}</div>""";

        content.Append($"""
            <tr><td style="padding:36px 48px 0 48px;text-align:center;">
              {eyebrow}
              <h1 style="margin:0;font-family:{SerifStack};font-size:26px;font-weight:600;color:{Ink};line-height:1.25;">{title}</h1>
            </td></tr>
            """);

        // Body copy
        content.Append($"""
            <tr><td style="padding:24px 48px 8px 48px;font-family:{SansStack};font-size:15px;line-height:1.75;color:{Body};">
              {RenderBody(composition.BodyText)}
            </td></tr>
            """);

        if (composition.LineItems.Count > 0)
            content.Append(RenderLineItems(composition.LineItems));

        if (composition.Totals.Count > 0)
            content.Append(RenderTotals(composition.Totals));

        if (composition.Details.Count > 0)
            content.Append(RenderDetails(composition.Details));

        if (composition.CallToAction is not null)
            content.Append(RenderCallToAction(composition.CallToAction));

        content.Append($"""
            <tr><td style="padding:8px 48px 44px 48px;"><div style="height:1px;background-color:{Hairline};line-height:1px;font-size:0;">&nbsp;</div></td></tr>
            <tr><td style="padding:0 48px 48px 48px;font-family:{SansStack};font-size:12px;line-height:1.7;color:{Muted};text-align:center;">
              {RenderFooter(storeName)}
            </td></tr>
            """);

        var preheader = Encode(composition.Title);

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <meta name="color-scheme" content="light only" />
              <title>{title}</title>
            </head>
            <body style="margin:0;padding:0;background-color:{PageBackground};">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:{PageBackground};font-size:1px;line-height:1px;">{preheader}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:{PageBackground};">
                <tr><td align="center" style="padding:32px 12px;">
                  <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="width:600px;max-width:100%;background-color:{Surface};border:1px solid {Border};">
                    {content}
                  </table>
                  <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="width:600px;max-width:100%;">
                    <tr><td style="padding:20px 12px 0 12px;font-family:{SansStack};font-size:11px;letter-spacing:1px;color:#b3b3b3;text-align:center;text-transform:uppercase;">{storeName}</td></tr>
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
            .Replace("\n\n", "</p><p style=\"margin:0 0 16px 0;\">", StringComparison.Ordinal)
            .Replace("\n", "<br />", StringComparison.Ordinal)
            .Insert(0, "<p style=\"margin:0 0 16px 0;\">")
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
                  <td width="76" style="padding:16px 16px 16px 0;border-bottom:1px solid {Hairline};vertical-align:top;">
                    <img src="{Encode(item.ImageUrl!)}" width="60" height="60" alt="{name}" style="display:block;width:60px;height:60px;object-fit:cover;border:1px solid {Border};" />
                  </td>
                  """;

            rows.Append($"""
                <tr>
                  {imageCell}
                  <td style="padding:16px 0;border-bottom:1px solid {Hairline};font-family:{SansStack};font-size:14px;color:{Ink};vertical-align:top;">
                    {name}{variant}
                    <br /><span style="color:{Muted};font-size:12px;letter-spacing:0.5px;">QTY {item.Quantity}</span>
                  </td>
                  <td style="padding:16px 0;border-bottom:1px solid {Hairline};font-family:{SansStack};font-size:14px;color:{Ink};text-align:right;white-space:nowrap;vertical-align:top;">{Encode(item.Amount)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:20px 48px 0 48px;">
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
            var labelStyle = isLast
                ? $"padding:14px 0 0 0;font-family:{SansStack};font-size:13px;font-weight:600;letter-spacing:1px;text-transform:uppercase;color:{Ink};border-top:1px solid {Ink};"
                : $"padding:6px 0;font-family:{SansStack};font-size:13px;color:{Muted};";
            var valueStyle = isLast
                ? $"padding:14px 0 0 0;font-family:{SerifStack};font-size:20px;font-weight:600;color:{Ink};text-align:right;white-space:nowrap;border-top:1px solid {Ink};"
                : $"padding:6px 0;font-family:{SansStack};font-size:14px;color:{Ink};text-align:right;white-space:nowrap;";

            rows.Append($"""
                <tr>
                  <td style="{labelStyle}">{Encode(entries[i].Label)}</td>
                  <td style="{valueStyle}">{Encode(entries[i].Value)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:18px 48px 8px 48px;">
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
                  <td style="padding:10px 0;font-family:{SansStack};font-size:12px;letter-spacing:0.5px;text-transform:uppercase;color:{Muted};">{Encode(row.Label)}</td>
                  <td style="padding:10px 0;font-family:{SansStack};font-size:14px;color:{Ink};text-align:right;">{Encode(row.Value)}</td>
                </tr>
                """);
        }

        return $"""
            <tr><td style="padding:18px 48px 8px 48px;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border:1px solid {Border};">
                <tr><td style="padding:6px 20px;">
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
            <tr><td style="padding:28px 48px 8px 48px;text-align:center;">
              <table role="presentation" cellpadding="0" cellspacing="0" align="center">
                <tr><td style="background-color:{Ink};">
                  <a href="{url}" style="display:inline-block;padding:15px 40px;font-family:{SansStack};font-size:12px;font-weight:600;letter-spacing:1.5px;text-transform:uppercase;color:#ffffff;text-decoration:none;">{text}</a>
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
            <span style="font-family:{SerifStack};font-size:15px;letter-spacing:2px;text-transform:uppercase;color:{Body};">{encodedStoreName}</span>
            <br /><br />You are receiving this email because of an action on your account.
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
