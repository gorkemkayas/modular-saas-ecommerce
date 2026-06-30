namespace Notification.Application.Notifications.Services;

public interface IEmailLayoutComposer
{
    string ComposeHtml(EmailComposition composition);
}

public sealed record EmailComposition(
    string StoreName,
    string Title,
    string BodyText,
    EmailCallToAction? CallToAction,
    IReadOnlyCollection<EmailDetailRow> Details,
    IReadOnlyCollection<EmailLineItem> LineItems,
    IReadOnlyCollection<EmailDetailRow> Totals);

public sealed record EmailCallToAction(
    string Text,
    string Url);

public sealed record EmailDetailRow(
    string Label,
    string Value);

public sealed record EmailLineItem(
    string Name,
    string? Variant,
    int Quantity,
    string Amount,
    string? ImageUrl = null);

public sealed record EmailContent(
    EmailCallToAction? CallToAction = null,
    IReadOnlyCollection<EmailDetailRow>? Details = null,
    IReadOnlyCollection<EmailLineItem>? LineItems = null,
    IReadOnlyCollection<EmailDetailRow>? Totals = null);
