using Notification.Application.Notifications.Services;

namespace Notification.Infrastructure.Services;

public sealed class MockEmailGateway : IEmailGateway
{
    public string ProviderName => "Mock";

    public Task<EmailSendResult> SendAsync(
        EmailSendRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestReference = $"mock-req-{Guid.NewGuid():N}";
        var messageId = $"mock-msg-{Guid.NewGuid():N}";

        return Task.FromResult(new EmailSendResult(
            true,
            requestReference,
            messageId,
            null,
            null));
    }
}
