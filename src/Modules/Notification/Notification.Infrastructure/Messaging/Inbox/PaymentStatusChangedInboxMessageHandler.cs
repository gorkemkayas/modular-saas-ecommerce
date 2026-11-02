using Notification.Application.Contracts;
using Notification.Application.Notifications.Services;
using Notification.Contracts;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence.Inbox;
using Payment.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class PaymentStatusChangedInboxMessageHandler : INotificationInboxMessageHandler
{
    private readonly QueuedNotificationSender _sender;
    private readonly NotificationModuleApi _notifications;
    private readonly NotificationRecipientResolver _recipients;
    public PaymentStatusChangedInboxMessageHandler(QueuedNotificationSender sender, NotificationRecipientResolver recipients)
    {
        _sender = sender;
        _notifications = new(sender);
        _recipients = recipients;
    }
    public string EventType => PaymentStatusChangedIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken = default)
    {
        var e = NotificationInboxEventReader.Read<PaymentStatusChangedIntegrationEvent>(message, EventType);
        if (!Enum.IsDefined(e.Kind))
            throw new InvalidDataException("Unsupported payment event kind.");
        var recipient = await _recipients.ResolveAsync(e.StoreId, e.OrderId, e.CustomerId,
            e.RecipientEmail, e.RecipientName, cancellationToken);
        switch (e.Kind)
        {
            case PaymentEventKind.Authorized:
                await _notifications.SendPaymentAuthorizedAsync(new(e.StoreId, e.PaymentId, e.OrderId,
                    e.CustomerId, e.OrderNumber, recipient.Email, recipient.FullName, e.Amount,
                    e.CurrencyCode, e.PaymentReference), cancellationToken);
                break;
            case PaymentEventKind.Captured:
                await _notifications.SendPaymentCapturedAsync(new(e.StoreId, e.PaymentId, e.OrderId,
                    e.CustomerId, e.OrderNumber, recipient.Email, recipient.FullName, e.Amount,
                    e.CurrencyCode, e.PaymentReference), cancellationToken);
                break;
            case PaymentEventKind.Failed:
                await _notifications.SendPaymentFailedAsync(new(e.StoreId, e.PaymentId, e.OrderId,
                    e.CustomerId, e.OrderNumber, recipient.Email, recipient.FullName, e.Amount,
                    e.CurrencyCode, e.Description), cancellationToken);
                break;
            case PaymentEventKind.Refunded:
                if (e.RefundId is null || e.RefundId == Guid.Empty)
                    throw new InvalidDataException("Refund notification requires a refund identity.");
                // Each partial refund is a separate business operation.
                await _sender.SendAsync(new TransactionalNotificationRequest(
                    e.StoreId, NotificationChannel.Email, NotificationTrigger.PaymentRefunded, "default",
                    "PaymentRefund", e.RefundId.Value, e.CustomerId, recipient.Email, recipient.FullName,
                    new Dictionary<string, string?>
                    {
                        ["RecipientName"] = recipient.FullName, ["OrderNumber"] = e.OrderNumber,
                        ["RefundedAmount"] = $"{e.Amount:#,##0.00} {e.CurrencyCode}",
                        ["CurrencyCode"] = e.CurrencyCode
                    }, new EmailContent(Details: new[]
                    {
                        new EmailDetailRow("Order No", e.OrderNumber),
                        new EmailDetailRow("Refunded Amount", $"{e.Amount:#,##0.00} {e.CurrencyCode}")
                    })), cancellationToken);
                break;
            default: throw new InvalidDataException("Unsupported payment event kind.");
        }
    }
}
