using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Abstractions.Queries;
using Order.Application.Integrations;
using Order.Contracts;
using Order.Domain.Repositories;

namespace Order.Application.Contracts;

public sealed class OrderModuleApi : IOrderModuleApi
{
    private readonly IOrderReadService _orderReadService;
    private readonly IOrderCustomerContextService _orderCustomerContextService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotificationService _notificationService;
    private readonly IOrderCatalogProductService _catalogProductService;
    private readonly ILogger<OrderModuleApi> _logger;

    public OrderModuleApi(
        IOrderReadService orderReadService,
        IOrderCustomerContextService orderCustomerContextService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOrderNotificationService notificationService,
        IOrderCatalogProductService catalogProductService,
        ILogger<OrderModuleApi> logger)
    {
        _orderReadService = orderReadService;
        _orderCustomerContextService = orderCustomerContextService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _catalogProductService = catalogProductService;
        _logger = logger;
    }

    public async Task<OrderPaymentContextResult?> GetCustomerOrderPaymentContextAsync(
        GetCustomerOrderPaymentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerIdentity = await _orderCustomerContextService.GetCustomerIdentityAsync(
            request.StoreId,
            request.ExternalUserId,
            cancellationToken);

        if (customerIdentity is null)
            return null;

        var order = await _orderReadService.GetByCustomerAsync(
            request.StoreId,
            customerIdentity.CustomerId,
            request.OrderId,
            cancellationToken);

        return order is null ? null : MapPaymentContext(order);
    }

    public async Task<OrderPaymentContextResult?> GetStoreOrderPaymentContextAsync(
        GetStoreOrderPaymentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderReadService.GetByIdAsync(
            request.StoreId,
            request.OrderId,
            cancellationToken);

        return order is null ? null : MapPaymentContext(order);
    }

    public async Task MarkPaymentAuthorizedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentAuthorized(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SendOrderConfirmationAsync(order, cancellationToken);
    }

    public async Task MarkPaymentCapturedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentCaptured(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SendOrderConfirmationAsync(order, cancellationToken);
    }

    public async Task MarkPaymentFailedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentFailed(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPaymentRefundedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentRefunded(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderShipmentContextResult?> GetCustomerOrderShipmentContextAsync(
        GetCustomerOrderShipmentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerIdentity = await _orderCustomerContextService.GetCustomerIdentityAsync(
            request.StoreId,
            request.ExternalUserId,
            cancellationToken);

        if (customerIdentity is null)
            return null;

        var order = await _orderReadService.GetByCustomerAsync(
            request.StoreId,
            customerIdentity.CustomerId,
            request.OrderId,
            cancellationToken);

        return order is null ? null : MapShipmentContext(order);
    }

    public async Task<OrderShipmentContextResult?> GetStoreOrderShipmentContextAsync(
        GetStoreOrderShipmentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderReadService.GetByIdAsync(
            request.StoreId,
            request.OrderId,
            cancellationToken);

        return order is null ? null : MapShipmentContext(order);
    }

    public async Task MarkShipmentCreatedAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request.StoreId, request.OrderId, cancellationToken);
        order.MarkShipmentCreated(request.ShipmentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkShippedAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request.StoreId, request.OrderId, cancellationToken);
        order.MarkShipped(request.ShipmentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDeliveredAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request.StoreId, request.OrderId, cancellationToken);
        order.MarkDelivered(request.ShipmentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkShipmentCancelledAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request.StoreId, request.OrderId, cancellationToken);
        order.MarkShipmentCancelled(request.ShipmentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order.Domain.Entities.Order> LoadOrderAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        return await LoadOrderAsync(request.StoreId, request.OrderId, cancellationToken);
    }

    // Sends the order confirmation email once payment has succeeded. Notification
    // dispatch is deduplicated by business key, so calling this on both the
    // authorized and captured transitions still results in a single email.
    private async Task SendOrderConfirmationAsync(
        Order.Domain.Entities.Order order,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = new List<OrderNotificationLineItem>(order.Items.Count);

            foreach (var item in order.Items)
            {
                var sellableItem = await _catalogProductService.GetSellableItemAsync(
                    order.StoreId,
                    item.ProductId,
                    item.ProductVariantId,
                    cancellationToken);

                items.Add(new OrderNotificationLineItem(
                    item.ProductName,
                    item.VariantName,
                    item.Quantity,
                    item.LineTotalAmount,
                    sellableItem?.ImageUrl));
            }

            await _notificationService.SendOrderPlacedAsync(
                order.StoreId,
                order.Id,
                order.CustomerId,
                order.OrderNumber.Value,
                order.CustomerSnapshot.Email,
                order.CustomerSnapshot.FullName,
                order.Totals.GrandTotalAmount,
                order.CurrencyCode,
                items,
                order.Totals.SubtotalAmount,
                order.Totals.ShippingAmount,
                cancellationToken);
        }
        catch (Exception notificationException)
        {
            _logger.LogWarning(
                notificationException,
                "Order confirmation notification failed | OrderId: {OrderId} | StoreId: {StoreId}",
                order.Id,
                order.StoreId);
        }
    }

    private async Task<Order.Domain.Entities.Order> LoadOrderAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdAsync(storeId, orderId, cancellationToken)
            ?? throw new Order.Application.Exceptions.OrderNotFoundException(orderId);
    }

    private static OrderPaymentContextResult MapPaymentContext(Order.Application.Orders.DTOs.OrderDto order)
    {
        return new OrderPaymentContextResult(
            order.Id,
            order.StoreId,
            order.CustomerId,
            order.OrderNumber,
            MapOrderStatus(order.Status),
            MapPaymentStatus(order.PaymentStatus),
            MapFulfillmentStatus(order.FulfillmentStatus),
            order.Totals.GrandTotalAmount,
            order.CurrencyCode,
            order.ReservationReference,
            new OrderPaymentCustomerResult(
                order.Customer.Email,
                order.Customer.FullName,
                order.Customer.PhoneNumber),
            new OrderPaymentAddressResult(
                order.BillingAddress.ContactName,
                order.BillingAddress.PhoneNumber,
                order.BillingAddress.Country,
                order.BillingAddress.City,
                order.BillingAddress.District,
                order.BillingAddress.Line1,
                order.BillingAddress.Line2,
                order.BillingAddress.PostalCode),
            new OrderPaymentAddressResult(
                order.ShippingAddress.ContactName,
                order.ShippingAddress.PhoneNumber,
                order.ShippingAddress.Country,
                order.ShippingAddress.City,
                order.ShippingAddress.District,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.PostalCode),
            order.Items
                .Select(item => new OrderPaymentItemResult(
                    item.ProductId,
                    item.ProductName,
                    item.VariantName,
                    item.Sku,
                    item.Quantity,
                    item.LineTotalAmount))
                .ToArray());
    }

    private static OrderShipmentContextResult MapShipmentContext(Order.Application.Orders.DTOs.OrderDto order)
    {
        return new OrderShipmentContextResult(
            order.Id,
            order.StoreId,
            order.CustomerId,
            order.OrderNumber,
            order.Customer.Email,
            order.Customer.FullName,
            MapOrderStatus(order.Status),
            MapPaymentStatus(order.PaymentStatus),
            MapFulfillmentStatus(order.FulfillmentStatus),
            order.ShipmentReference,
            new OrderShipmentAddressResult(
                order.ShippingAddress.ContactName,
                order.ShippingAddress.PhoneNumber,
                order.ShippingAddress.Country,
                order.ShippingAddress.City,
                order.ShippingAddress.District,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.PostalCode),
            order.ShippingCarrier is null
                ? null
                : new OrderShipmentCarrierResult(
                    order.ShippingCarrier.CarrierId,
                    order.ShippingCarrier.Code,
                    order.ShippingCarrier.Name,
                    order.ShippingCarrier.ServiceCode,
                    order.ShippingCarrier.ServiceName,
                    order.ShippingCarrier.TrackingUrl),
            order.Items
                .Select(item => new OrderShipmentItemResult(
                    item.Id,
                    item.ProductId,
                    item.ProductVariantId,
                    item.ProductName,
                    item.VariantName,
                    item.Sku,
                    item.Quantity))
                .ToArray());
    }

    private static OrderStatusContract MapOrderStatus(Order.Domain.Enums.OrderStatus status)
    {
        return status switch
        {
            Order.Domain.Enums.OrderStatus.Pending => OrderStatusContract.Pending,
            Order.Domain.Enums.OrderStatus.Confirmed => OrderStatusContract.Confirmed,
            Order.Domain.Enums.OrderStatus.Cancelled => OrderStatusContract.Cancelled,
            Order.Domain.Enums.OrderStatus.Completed => OrderStatusContract.Completed,
            _ => throw new InvalidOperationException($"Unsupported order status '{status}'.")
        };
    }

    private static OrderPaymentStatusContract MapPaymentStatus(Order.Domain.Enums.PaymentStatus status)
    {
        return status switch
        {
            Order.Domain.Enums.PaymentStatus.Pending => OrderPaymentStatusContract.Pending,
            Order.Domain.Enums.PaymentStatus.Authorized => OrderPaymentStatusContract.Authorized,
            Order.Domain.Enums.PaymentStatus.Captured => OrderPaymentStatusContract.Captured,
            Order.Domain.Enums.PaymentStatus.Failed => OrderPaymentStatusContract.Failed,
            Order.Domain.Enums.PaymentStatus.Refunded => OrderPaymentStatusContract.Refunded,
            _ => throw new InvalidOperationException($"Unsupported order payment status '{status}'.")
        };
    }

    private static OrderFulfillmentStatusContract MapFulfillmentStatus(Order.Domain.Enums.FulfillmentStatus status)
    {
        return status switch
        {
            Order.Domain.Enums.FulfillmentStatus.Unfulfilled => OrderFulfillmentStatusContract.Unfulfilled,
            Order.Domain.Enums.FulfillmentStatus.PartiallyFulfilled => OrderFulfillmentStatusContract.PartiallyFulfilled,
            Order.Domain.Enums.FulfillmentStatus.Fulfilled => OrderFulfillmentStatusContract.Fulfilled,
            Order.Domain.Enums.FulfillmentStatus.Shipped => OrderFulfillmentStatusContract.Shipped,
            Order.Domain.Enums.FulfillmentStatus.Delivered => OrderFulfillmentStatusContract.Delivered,
            Order.Domain.Enums.FulfillmentStatus.Returned => OrderFulfillmentStatusContract.Returned,
            _ => throw new InvalidOperationException($"Unsupported order fulfillment status '{status}'.")
        };
    }
}
