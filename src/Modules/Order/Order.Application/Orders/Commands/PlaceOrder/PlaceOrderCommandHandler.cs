using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Exceptions;
using Order.Application.Integrations;
using Order.Domain.Entities;
using Order.Domain.Models;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly IOrderCustomerContextService _customerContextService;
    private readonly IOrderCatalogProductService _catalogProductService;
    private readonly IOrderPricingService _pricingService;
    private readonly IOrderInventoryService _inventoryService;
    private readonly IOrderShippingCarrierService _shippingCarrierService;
    private readonly IOrderNotificationService _notificationService;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOrderNumberGenerator orderNumberGenerator,
        IOrderCustomerContextService customerContextService,
        IOrderCatalogProductService catalogProductService,
        IOrderPricingService pricingService,
        IOrderInventoryService inventoryService,
        IOrderShippingCarrierService shippingCarrierService,
        IOrderNotificationService notificationService,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _orderNumberGenerator = orderNumberGenerator;
        _customerContextService = customerContextService;
        _catalogProductService = catalogProductService;
        _pricingService = pricingService;
        _inventoryService = inventoryService;
        _shippingCarrierService = shippingCarrierService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Guid> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new OrderValidationException("StoreId is required.");

        if (command.ExternalUserId == Guid.Empty)
            throw new UnauthorizedOrderAccessException();

        if (command.ShippingAddressId == Guid.Empty)
            throw new OrderValidationException("ShippingAddressId is required.");

        if (command.ShippingCarrierId == Guid.Empty)
            throw new OrderValidationException("ShippingCarrierId is required.");

        if (string.IsNullOrWhiteSpace(command.CurrencyCode))
            throw new OrderValidationException("CurrencyCode is required.");

        if (command.Items.Count == 0)
            throw new OrderValidationException("Order must contain at least one item.");

        if (command.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0))
            throw new OrderValidationException("Each order item must have a valid product and quantity.");

        var shippingCarrier = await _shippingCarrierService.GetActiveCarrierAsync(
            command.StoreId,
            command.ShippingCarrierId,
            cancellationToken);

        if (shippingCarrier is null)
            throw new OrderValidationException("Selected shipping carrier is not available for this store.");

        var customerContext = await _customerContextService.GetCustomerContextAsync(
            command.StoreId,
            command.ExternalUserId,
            command.ShippingAddressId,
            command.BillingAddressId,
            cancellationToken);

        if (customerContext is null)
            throw new UnauthorizedOrderAccessException();

        var inventoryItems = command.Items
            .Select(x => new OrderInventoryItemRequest(x.ProductId, x.ProductVariantId, x.Quantity))
            .ToArray();

        await _inventoryService.EnsureAvailabilityAsync(command.StoreId, inventoryItems, cancellationToken);

        var itemDrafts = new List<OrderItemDraft>(command.Items.Count);
        var imageUrls = new Dictionary<(Guid ProductId, Guid? ProductVariantId), string?>();
        var currencyCode = command.CurrencyCode.Trim().ToUpperInvariant();

        foreach (var item in command.Items)
        {
            var sellableItem = await _catalogProductService.GetSellableItemAsync(
                command.StoreId,
                item.ProductId,
                item.ProductVariantId,
                cancellationToken);

            if (sellableItem is null)
                throw new OrderCatalogItemUnavailableException(item.ProductId, item.ProductVariantId);

            imageUrls[(item.ProductId, item.ProductVariantId)] = sellableItem.ImageUrl;

            var resolvedPrice = await _pricingService.ResolvePriceAsync(
                command.StoreId,
                item.ProductId,
                item.ProductVariantId,
                currencyCode,
                cancellationToken);

            if (resolvedPrice is null)
                throw new OrderPricingUnavailableException(item.ProductId, item.ProductVariantId, currencyCode);

            itemDrafts.Add(new OrderItemDraft(
                item.ProductId,
                item.ProductVariantId,
                sellableItem.ProductName,
                sellableItem.VariantName,
                sellableItem.Sku,
                item.Quantity,
                OrderPriceSnapshot.Create(
                    resolvedPrice.Amount,
                    resolvedPrice.CurrencyCode,
                    resolvedPrice.CompareAtAmount,
                    resolvedPrice.PriceListId,
                    resolvedPrice.PriceEntryId)));
        }

        var orderNumber = OrderNumber.Create(await _orderNumberGenerator.GenerateAsync(command.StoreId, cancellationToken));

        var order = Order.Domain.Entities.Order.Place(
            command.StoreId,
            orderNumber,
            CustomerSnapshot.Create(
                customerContext.CustomerId,
                customerContext.Email,
                customerContext.FullName,
                customerContext.PhoneNumber),
            OrderAddressSnapshot.Create(
                customerContext.BillingAddress.Title,
                customerContext.BillingAddress.ContactName,
                customerContext.BillingAddress.PhoneNumber,
                customerContext.BillingAddress.Country,
                customerContext.BillingAddress.City,
                customerContext.BillingAddress.District,
                customerContext.BillingAddress.Line1,
                customerContext.BillingAddress.Line2,
                customerContext.BillingAddress.PostalCode),
            OrderAddressSnapshot.Create(
                customerContext.ShippingAddress.Title,
                customerContext.ShippingAddress.ContactName,
                customerContext.ShippingAddress.PhoneNumber,
                customerContext.ShippingAddress.Country,
                customerContext.ShippingAddress.City,
                customerContext.ShippingAddress.District,
                customerContext.ShippingAddress.Line1,
                customerContext.ShippingAddress.Line2,
                customerContext.ShippingAddress.PostalCode),
            currencyCode,
            itemDrafts,
            ShippingCarrierSnapshot.Create(
                shippingCarrier.Id,
                shippingCarrier.Code,
                shippingCarrier.Name,
                shippingCarrier.ServiceCode,
                shippingCarrier.ServiceName,
                shippingCarrier.TrackingUrl));

        var reservationReference = $"ord-{order.Id:N}";

        await _inventoryService.ReserveAsync(
            command.StoreId,
            order.Id,
            reservationReference,
            inventoryItems,
            cancellationToken);

        order.SetReservationReference(reservationReference);

        try
        {
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await _inventoryService.ReleaseReservationAsync(
                    command.StoreId,
                    reservationReference,
                    "Order persistence failed after reservation.",
                    CancellationToken.None);
            }
            catch (Exception releaseException)
            {
                _logger.LogWarning(
                    releaseException,
                    "Inventory reservation compensation failed | ReservationReference: {ReservationReference} | StoreId: {StoreId}",
                    reservationReference,
                    command.StoreId);
            }

            throw;
        }

        _logger.LogInformation(
            "Order placed | OrderId: {OrderId} | OrderNumber: {OrderNumber} | StoreId: {StoreId} | CustomerId: {CustomerId} | Total: {GrandTotal}",
            order.Id,
            order.OrderNumber.Value,
            order.StoreId,
            order.CustomerId,
            order.Totals.GrandTotalAmount);

        try
        {
            await _notificationService.SendOrderPlacedAsync(
                order.StoreId,
                order.Id,
                order.CustomerId,
                order.OrderNumber.Value,
                order.CustomerSnapshot.Email,
                order.CustomerSnapshot.FullName,
                order.Totals.GrandTotalAmount,
                order.CurrencyCode,
                order.Items
                    .Select(item => new OrderNotificationLineItem(
                        item.ProductName,
                        item.VariantName,
                        item.Quantity,
                        item.LineTotalAmount,
                        imageUrls.TryGetValue((item.ProductId, item.ProductVariantId), out var imageUrl)
                            ? imageUrl
                            : null))
                    .ToArray(),
                order.Totals.SubtotalAmount,
                order.Totals.ShippingAmount,
                cancellationToken);
        }
        catch (Exception notificationException)
        {
            _logger.LogWarning(
                notificationException,
                "Order placed notification failed | OrderId: {OrderId} | StoreId: {StoreId}",
                order.Id,
                order.StoreId);
        }

        return order.Id;
    }
}
