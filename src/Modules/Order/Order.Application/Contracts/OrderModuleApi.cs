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

    public OrderModuleApi(
        IOrderReadService orderReadService,
        IOrderCustomerContextService orderCustomerContextService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderReadService = orderReadService;
        _orderCustomerContextService = orderCustomerContextService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
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

        return order is null
            ? null
            : new OrderPaymentContextResult(
                order.Id,
                order.StoreId,
                order.CustomerId,
                order.OrderNumber,
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

    public async Task<OrderPaymentContextResult?> GetStoreOrderPaymentContextAsync(
        GetStoreOrderPaymentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderReadService.GetByIdAsync(
            request.StoreId,
            request.OrderId,
            cancellationToken);

        return order is null
            ? null
            : new OrderPaymentContextResult(
                order.Id,
                order.StoreId,
                order.CustomerId,
                order.OrderNumber,
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

    public async Task MarkPaymentAuthorizedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentAuthorized(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPaymentCapturedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(request, cancellationToken);
        order.MarkPaymentCaptured(request.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

    private async Task<Order.Domain.Entities.Order> LoadOrderAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdAsync(request.StoreId, request.OrderId, cancellationToken)
            ?? throw new Order.Application.Exceptions.OrderNotFoundException(request.OrderId);
    }
}
