namespace Notification.Application.Notifications.Services;

public interface IEmailGateway
{
    string ProviderName { get; }

    Task<EmailSendResult> SendAsync(
        EmailSendRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record EmailSendRequest(
    Guid StoreId,
    string ToEmail,
    string? ToName,
    string Subject,
    string Body,
    string HtmlBody);

public sealed record EmailSendResult(
    bool IsSuccess,
    string? ProviderRequestReference,
    string? ProviderMessageId,
    string? FailureCode,
    string? FailureMessage);
