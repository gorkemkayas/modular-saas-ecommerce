using Notification.Domain.Enums;

namespace Notification.Application.Notifications.Services;

public interface INotificationSender
{
    Task<Guid> SendAsync(
        TransactionalNotificationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record TransactionalNotificationRequest(
    Guid StoreId,
    NotificationChannel Channel,
    NotificationTrigger Trigger,
    string Locale,
    string BusinessEntityType,
    Guid BusinessEntityId,
    Guid? CustomerId,
    string? RecipientAddress,
    string? RecipientName,
    IReadOnlyDictionary<string, string?> Tokens,
    EmailContent? Content = null);
