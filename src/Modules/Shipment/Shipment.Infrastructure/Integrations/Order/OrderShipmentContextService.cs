using Order.Contracts;
using Shipment.Application.Integrations;

namespace Shipment.Infrastructure.Integrations.Order;

public sealed class OrderShipmentContextService : IOrderShipmentContextService
{
    private readonly IOrderModuleApi _orderModuleApi;

    public OrderShipmentContextService(IOrderModuleApi orderModuleApi)
    {
        _orderModuleApi = orderModuleApi;
    }

    public async Task<OrderShipmentContext?> GetCustomerOrderContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var context = await _orderModuleApi.GetCustomerOrderShipmentContextAsync(
            new GetCustomerOrderShipmentContextRequest(storeId, externalUserId, orderId),
            cancellationToken);

        return context is null ? null : Map(context);
    }

    public async Task<OrderShipmentContext?> GetStoreOrderContextAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var context = await _orderModuleApi.GetStoreOrderShipmentContextAsync(
            new GetStoreOrderShipmentContextRequest(storeId, orderId),
            cancellationToken);

        return context is null ? null : Map(context);
    }

    private static OrderShipmentContext Map(OrderShipmentContextResult context)
    {
        return new OrderShipmentContext(
            context.OrderId,
            context.StoreId,
            context.CustomerId,
            context.OrderNumber,
            context.CustomerEmail,
            context.CustomerFullName,
            MapOrderStatus(context.Status),
            MapPaymentStatus(context.PaymentStatus),
            MapFulfillmentStatus(context.FulfillmentStatus),
            context.ShipmentReference,
            new OrderShipmentAddress(
                context.ShippingAddress.ContactName,
                context.ShippingAddress.PhoneNumber,
                context.ShippingAddress.Country,
                context.ShippingAddress.City,
                context.ShippingAddress.District,
                context.ShippingAddress.Line1,
                context.ShippingAddress.Line2,
                context.ShippingAddress.PostalCode),
            context.ShippingCarrier is null
                ? null
                : new OrderShipmentCarrier(
                    context.ShippingCarrier.CarrierId,
                    context.ShippingCarrier.Code,
                    context.ShippingCarrier.Name,
                    context.ShippingCarrier.ServiceCode,
                    context.ShippingCarrier.ServiceName,
                    context.ShippingCarrier.TrackingUrl),
            context.Items
                .Select(item => new OrderShipmentItem(
                    item.OrderItemId,
                    item.ProductId,
                    item.ProductVariantId,
                    item.ProductName,
                    item.VariantName,
                    item.Sku,
                    item.Quantity))
                .ToArray());
    }

    private static Shipment.Application.Integrations.OrderShipmentStatus MapOrderStatus(OrderStatusContract status)
    {
        return status switch
        {
            OrderStatusContract.Pending => Shipment.Application.Integrations.OrderShipmentStatus.Pending,
            OrderStatusContract.Confirmed => Shipment.Application.Integrations.OrderShipmentStatus.Confirmed,
            OrderStatusContract.Cancelled => Shipment.Application.Integrations.OrderShipmentStatus.Cancelled,
            OrderStatusContract.Completed => Shipment.Application.Integrations.OrderShipmentStatus.Completed,
            _ => throw new InvalidOperationException($"Unsupported order status '{status}'.")
        };
    }

    private static Shipment.Application.Integrations.OrderShipmentPaymentStatus MapPaymentStatus(OrderPaymentStatusContract status)
    {
        return status switch
        {
            OrderPaymentStatusContract.Pending => Shipment.Application.Integrations.OrderShipmentPaymentStatus.Pending,
            OrderPaymentStatusContract.Authorized => Shipment.Application.Integrations.OrderShipmentPaymentStatus.Authorized,
            OrderPaymentStatusContract.Captured => Shipment.Application.Integrations.OrderShipmentPaymentStatus.Captured,
            OrderPaymentStatusContract.Failed => Shipment.Application.Integrations.OrderShipmentPaymentStatus.Failed,
            OrderPaymentStatusContract.Refunded => Shipment.Application.Integrations.OrderShipmentPaymentStatus.Refunded,
            _ => throw new InvalidOperationException($"Unsupported order payment status '{status}'.")
        };
    }

    private static Shipment.Application.Integrations.OrderShipmentFulfillmentStatus MapFulfillmentStatus(OrderFulfillmentStatusContract status)
    {
        return status switch
        {
            OrderFulfillmentStatusContract.Unfulfilled => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.Unfulfilled,
            OrderFulfillmentStatusContract.PartiallyFulfilled => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.PartiallyFulfilled,
            OrderFulfillmentStatusContract.Fulfilled => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.Fulfilled,
            OrderFulfillmentStatusContract.Shipped => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.Shipped,
            OrderFulfillmentStatusContract.Delivered => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.Delivered,
            OrderFulfillmentStatusContract.Returned => Shipment.Application.Integrations.OrderShipmentFulfillmentStatus.Returned,
            _ => throw new InvalidOperationException($"Unsupported order fulfillment status '{status}'.")
        };
    }
}
