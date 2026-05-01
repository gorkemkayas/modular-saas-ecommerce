using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Application.Notifications.Services;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Commands.ProcessResendWebhook;

public sealed class ProcessResendWebhookCommandHandler : IRequestHandler<ProcessResendWebhookCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly INotificationDispatchRepository _dispatchRepository;
    private readonly IResendWebhookVerifier _webhookVerifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessResendWebhookCommandHandler> _logger;

    public ProcessResendWebhookCommandHandler(
        INotificationDispatchRepository dispatchRepository,
        IResendWebhookVerifier webhookVerifier,
        IUnitOfWork unitOfWork,
        ILogger<ProcessResendWebhookCommandHandler> logger)
    {
        _dispatchRepository = dispatchRepository;
        _webhookVerifier = webhookVerifier;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProcessResendWebhookCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Payload))
            throw new NotificationWebhookValidationException("Webhook payload is required.");

        if (_webhookVerifier.IsVerificationEnabled
            && !_webhookVerifier.Verify(
                command.Payload,
                command.WebhookId,
                command.WebhookTimestamp,
                command.WebhookSignature))
        {
            throw new NotificationWebhookValidationException("Webhook signature is invalid.");
        }

        var webhook = JsonSerializer.Deserialize<ResendWebhookPayload>(command.Payload, JsonOptions)
            ?? throw new NotificationWebhookValidationException("Webhook payload could not be parsed.");

        if (string.IsNullOrWhiteSpace(webhook.Type))
            throw new NotificationWebhookValidationException("Webhook event type is required.");

        if (string.IsNullOrWhiteSpace(webhook.Data?.EmailId))
        {
            _logger.LogInformation(
                "Resend webhook ignored because email id was missing | EventType: {EventType}",
                webhook.Type);
            return;
        }

        var dispatch = await _dispatchRepository.GetByProviderMessageIdAsync(
            "Resend",
            webhook.Data.EmailId,
            cancellationToken);

        if (dispatch is null)
        {
            _logger.LogInformation(
                "Resend webhook ignored because dispatch was not found | EventType: {EventType} | ProviderMessageId: {ProviderMessageId}",
                webhook.Type,
                webhook.Data.EmailId);
            return;
        }

        var occurredAtUtc = webhook.CreatedAtUtc ?? webhook.Data.CreatedAtUtc ?? DateTime.UtcNow;

        switch (webhook.Type)
        {
            case "email.sent":
            case "email.delivery_delayed":
            case "email.suppressed":
                dispatch.RegisterProviderEvent(occurredAtUtc, webhook.Type);
                break;

            case "email.delivered":
                dispatch.RegisterDeliveredEvent(occurredAtUtc, webhook.Type);
                break;

            case "email.opened":
                dispatch.RegisterOpenedEvent(occurredAtUtc, webhook.Type);
                break;

            case "email.clicked":
                dispatch.RegisterClickedEvent(occurredAtUtc, webhook.Type);
                break;

            case "email.bounced":
                dispatch.RegisterBouncedEvent(
                    occurredAtUtc,
                    webhook.Type,
                    webhook.Data.Bounce is null ? null : $"{webhook.Data.Bounce.Type}:{webhook.Data.Bounce.SubType}",
                    webhook.Data.Bounce?.Message);
                break;

            case "email.complained":
                dispatch.RegisterComplainedEvent(
                    occurredAtUtc,
                    webhook.Type,
                    "Email marked as spam by the recipient.");
                break;

            case "email.failed":
                dispatch.RegisterBouncedEvent(
                    occurredAtUtc,
                    webhook.Type,
                    "provider_failed",
                    "Provider reported a send failure after the initial request.");
                break;

            default:
                _logger.LogInformation(
                    "Resend webhook ignored because the event type is not currently handled | EventType: {EventType}",
                    webhook.Type);
                return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed class ResendWebhookPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAtUtc { get; init; }

        [JsonPropertyName("data")]
        public ResendWebhookData? Data { get; init; }
    }

    private sealed class ResendWebhookData
    {
        [JsonPropertyName("email_id")]
        public string EmailId { get; init; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAtUtc { get; init; }

        [JsonPropertyName("bounce")]
        public ResendBouncePayload? Bounce { get; init; }
    }

    private sealed class ResendBouncePayload
    {
        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("subType")]
        public string? SubType { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
