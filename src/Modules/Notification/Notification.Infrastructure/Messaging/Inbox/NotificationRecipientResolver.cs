using Notification.Application.Abstractions;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class NotificationRecipientResolver(IOrderRecipientReader legacyReader)
{
    public async Task<OrderNotificationRecipient> ResolveAsync(Guid storeId, Guid orderId, Guid? customerId,
        string? email, string? fullName, CancellationToken cancellationToken = default)
    {
        if (customerId is { } id && id != Guid.Empty && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(fullName))
            return new(id, email, fullName);

        // Legacy producer rows/events have no contact snapshot. Only Inbox processing
        // may depend on this read; business persistence in the producer never does.
        var recipient = await legacyReader.GetAsync(storeId, orderId, cancellationToken)
            ?? throw new InvalidOperationException("Legacy event recipient is not available yet.");
        if (customerId.HasValue && customerId != recipient.CustomerId)
            throw new InvalidDataException("Legacy recipient does not belong to the event customer.");
        return recipient;
    }
}
