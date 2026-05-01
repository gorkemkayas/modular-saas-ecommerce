using MediatR;

namespace Notification.Application.Notifications.Commands.ProcessResendWebhook;

public sealed record ProcessResendWebhookCommand(
    string Payload,
    string? WebhookId,
    string? WebhookTimestamp,
    string? WebhookSignature) : IRequest;
