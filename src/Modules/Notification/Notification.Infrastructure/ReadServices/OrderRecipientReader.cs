using Notification.Application.Abstractions;
using Order.Contracts;

namespace Notification.Infrastructure.ReadServices;

public sealed class OrderRecipientReader(IOrderModuleApi orders) : IOrderRecipientReader
{
    public async Task<OrderNotificationRecipient?> GetAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var context = await orders.GetStoreOrderPaymentContextAsync(new(storeId, orderId), cancellationToken);
        if (context is null) return null;
        if (context.StoreId != storeId || context.OrderId != orderId)
            throw new InvalidDataException("Order recipient context does not match the requested tenant and order.");
        return new(context.CustomerId, context.Customer.Email, context.Customer.FullName);
    }
}
