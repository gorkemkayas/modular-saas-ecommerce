namespace Notification.Application.Abstractions;

public interface IOrderRecipientReader
{
    Task<OrderNotificationRecipient?> GetAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}

public sealed record OrderNotificationRecipient(Guid CustomerId, string Email, string FullName);
