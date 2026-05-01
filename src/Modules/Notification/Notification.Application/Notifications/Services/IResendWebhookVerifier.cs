namespace Notification.Application.Notifications.Services;

public interface IResendWebhookVerifier
{
    bool IsVerificationEnabled { get; }

    bool Verify(
        string payload,
        string? webhookId,
        string? webhookTimestamp,
        string? webhookSignature);
}
