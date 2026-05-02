using Order.Contracts;
using Payment.Application.Integrations;

namespace Payment.Infrastructure.Integrations.Order;

public sealed class OrderPaymentContextService : IOrderPaymentContextService
{
    private readonly IOrderModuleApi _orderModuleApi;

    public OrderPaymentContextService(IOrderModuleApi orderModuleApi)
    {
        _orderModuleApi = orderModuleApi;
    }

    public async Task<OrderPaymentContext?> GetCustomerOrderContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var context = await _orderModuleApi.GetCustomerOrderPaymentContextAsync(
            new GetCustomerOrderPaymentContextRequest(storeId, externalUserId, orderId),
            cancellationToken);

        return context is null
            ? null
            : new OrderPaymentContext(
                context.OrderId,
                context.StoreId,
                context.CustomerId,
                context.OrderNumber,
                MapOrderStatus(context.Status),
                MapPaymentStatus(context.PaymentStatus),
                MapFulfillmentStatus(context.FulfillmentStatus),
                context.GrandTotalAmount,
                context.CurrencyCode,
                context.ReservationReference,
                new OrderPaymentCustomer(
                    context.Customer.Email,
                    context.Customer.FullName,
                    context.Customer.PhoneNumber),
                new OrderPaymentAddress(
                    context.BillingAddress.ContactName,
                    context.BillingAddress.PhoneNumber,
                    context.BillingAddress.Country,
                    context.BillingAddress.City,
                    context.BillingAddress.District,
                    context.BillingAddress.Line1,
                    context.BillingAddress.Line2,
                    context.BillingAddress.PostalCode),
                new OrderPaymentAddress(
                    context.ShippingAddress.ContactName,
                    context.ShippingAddress.PhoneNumber,
                    context.ShippingAddress.Country,
                    context.ShippingAddress.City,
                    context.ShippingAddress.District,
                    context.ShippingAddress.Line1,
                    context.ShippingAddress.Line2,
                    context.ShippingAddress.PostalCode),
                context.Items
                    .Select(item => new OrderPaymentItem(
                        item.ProductId,
                        item.ProductName,
                        item.VariantName,
                        item.Sku,
                        item.Quantity,
                        item.LineTotalAmount))
                    .ToArray());
    }

    public async Task<OrderPaymentContext?> GetStoreOrderContextAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var context = await _orderModuleApi.GetStoreOrderPaymentContextAsync(
            new GetStoreOrderPaymentContextRequest(storeId, orderId),
            cancellationToken);

        return context is null
            ? null
            : new OrderPaymentContext(
                context.OrderId,
                context.StoreId,
                context.CustomerId,
                context.OrderNumber,
                MapOrderStatus(context.Status),
                MapPaymentStatus(context.PaymentStatus),
                MapFulfillmentStatus(context.FulfillmentStatus),
                context.GrandTotalAmount,
                context.CurrencyCode,
                context.ReservationReference,
                new OrderPaymentCustomer(
                    context.Customer.Email,
                    context.Customer.FullName,
                    context.Customer.PhoneNumber),
                new OrderPaymentAddress(
                    context.BillingAddress.ContactName,
                    context.BillingAddress.PhoneNumber,
                    context.BillingAddress.Country,
                    context.BillingAddress.City,
                    context.BillingAddress.District,
                    context.BillingAddress.Line1,
                    context.BillingAddress.Line2,
                    context.BillingAddress.PostalCode),
                new OrderPaymentAddress(
                    context.ShippingAddress.ContactName,
                    context.ShippingAddress.PhoneNumber,
                    context.ShippingAddress.Country,
                    context.ShippingAddress.City,
                    context.ShippingAddress.District,
                    context.ShippingAddress.Line1,
                    context.ShippingAddress.Line2,
                    context.ShippingAddress.PostalCode),
                context.Items
                    .Select(item => new OrderPaymentItem(
                        item.ProductId,
                        item.ProductName,
                        item.VariantName,
                        item.Sku,
                        item.Quantity,
                        item.LineTotalAmount))
                    .ToArray());
    }

    private static OrderLifecycleStatus MapOrderStatus(OrderStatusContract status)
    {
        return status switch
        {
            OrderStatusContract.Pending => OrderLifecycleStatus.Pending,
            OrderStatusContract.Confirmed => OrderLifecycleStatus.Confirmed,
            OrderStatusContract.Cancelled => OrderLifecycleStatus.Cancelled,
            OrderStatusContract.Completed => OrderLifecycleStatus.Completed,
            _ => throw new InvalidOperationException($"Unsupported order status '{status}'.")
        };
    }

    private static OrderPaymentLifecycleStatus MapPaymentStatus(OrderPaymentStatusContract status)
    {
        return status switch
        {
            OrderPaymentStatusContract.Pending => OrderPaymentLifecycleStatus.Pending,
            OrderPaymentStatusContract.Authorized => OrderPaymentLifecycleStatus.Authorized,
            OrderPaymentStatusContract.Captured => OrderPaymentLifecycleStatus.Captured,
            OrderPaymentStatusContract.Failed => OrderPaymentLifecycleStatus.Failed,
            OrderPaymentStatusContract.Refunded => OrderPaymentLifecycleStatus.Refunded,
            _ => throw new InvalidOperationException($"Unsupported order payment status '{status}'.")
        };
    }

    private static OrderFulfillmentLifecycleStatus MapFulfillmentStatus(OrderFulfillmentStatusContract status)
    {
        return status switch
        {
            OrderFulfillmentStatusContract.Unfulfilled => OrderFulfillmentLifecycleStatus.Unfulfilled,
            OrderFulfillmentStatusContract.PartiallyFulfilled => OrderFulfillmentLifecycleStatus.PartiallyFulfilled,
            OrderFulfillmentStatusContract.Fulfilled => OrderFulfillmentLifecycleStatus.Fulfilled,
            OrderFulfillmentStatusContract.Shipped => OrderFulfillmentLifecycleStatus.Shipped,
            OrderFulfillmentStatusContract.Delivered => OrderFulfillmentLifecycleStatus.Delivered,
            OrderFulfillmentStatusContract.Returned => OrderFulfillmentLifecycleStatus.Returned,
            _ => throw new InvalidOperationException($"Unsupported order fulfillment status '{status}'.")
        };
    }
}
