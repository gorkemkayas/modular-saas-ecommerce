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
            (Shipment.Application.Integrations.OrderShipmentStatus)context.Status,
            (Shipment.Application.Integrations.OrderShipmentPaymentStatus)context.PaymentStatus,
            (Shipment.Application.Integrations.OrderShipmentFulfillmentStatus)context.FulfillmentStatus,
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
}
