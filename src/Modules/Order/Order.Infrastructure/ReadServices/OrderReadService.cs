using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Queries;
using Order.Application.Common.Models;
using Order.Application.Orders.DTOs;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.ReadServices;

public sealed class OrderReadService : IOrderReadService
{
    private readonly OrderDbContext _context;

    public OrderReadService(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderSummaryDto>> SearchByCustomerAsync(
        Guid storeId,
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(x => x.StoreId == storeId && x.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderSummaryDto(
                x.Id,
                x.OrderNumber.Value,
                x.Status,
                x.PaymentStatus,
                x.FulfillmentStatus,
                x.CurrencyCode,
                x.ShippingCarrierSnapshot != null ? x.ShippingCarrierSnapshot.Name : null,
                x.Items.Count,
                x.Totals.GrandTotalAmount,
                x.PlacedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<OrderSummaryDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<OrderDto?> GetByCustomerAsync(
        Guid storeId,
        Guid customerId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.StoreId == storeId && x.CustomerId == customerId && x.Id == orderId,
                cancellationToken);

        return order is null ? null : Map(order);
    }

    public async Task<OrderDto?> GetByIdAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == orderId, cancellationToken);

        return order is null ? null : Map(order);
    }

    private static OrderDto Map(Order.Domain.Entities.Order order)
    {
        return new OrderDto(
            order.Id,
            order.StoreId,
            order.CustomerId,
            order.OrderNumber.Value,
            order.Status,
            order.PaymentStatus,
            order.FulfillmentStatus,
            order.CurrencyCode,
            new OrderCustomerSnapshotDto(
                order.CustomerSnapshot.CustomerId,
                order.CustomerSnapshot.Email,
                order.CustomerSnapshot.FullName,
                order.CustomerSnapshot.PhoneNumber),
            new OrderAddressSnapshotDto(
                order.BillingAddressSnapshot.Title,
                order.BillingAddressSnapshot.ContactName,
                order.BillingAddressSnapshot.PhoneNumber,
                order.BillingAddressSnapshot.Country,
                order.BillingAddressSnapshot.City,
                order.BillingAddressSnapshot.District,
                order.BillingAddressSnapshot.Line1,
                order.BillingAddressSnapshot.Line2,
                order.BillingAddressSnapshot.PostalCode),
            new OrderAddressSnapshotDto(
                order.ShippingAddressSnapshot.Title,
                order.ShippingAddressSnapshot.ContactName,
                order.ShippingAddressSnapshot.PhoneNumber,
                order.ShippingAddressSnapshot.Country,
                order.ShippingAddressSnapshot.City,
                order.ShippingAddressSnapshot.District,
                order.ShippingAddressSnapshot.Line1,
                order.ShippingAddressSnapshot.Line2,
                order.ShippingAddressSnapshot.PostalCode),
            order.ShippingCarrierSnapshot is null
                ? null
                : new OrderShippingCarrierSnapshotDto(
                    order.ShippingCarrierSnapshot.CarrierId,
                    order.ShippingCarrierSnapshot.Code,
                    order.ShippingCarrierSnapshot.Name,
                    order.ShippingCarrierSnapshot.ServiceCode,
                    order.ShippingCarrierSnapshot.ServiceName,
                    order.ShippingCarrierSnapshot.TrackingUrl),
            new OrderTotalsDto(
                order.Totals.SubtotalAmount,
                order.Totals.DiscountAmount,
                order.Totals.ShippingAmount,
                order.Totals.TaxAmount,
                order.Totals.GrandTotalAmount),
            order.PlacedAtUtc,
            order.CancelledAtUtc,
            order.CancellationReason,
            order.ReservationReference,
            order.PaymentReference,
            order.ShipmentReference,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.Items
                .Select(x => new OrderItemDto(
                    x.Id,
                    x.ProductId,
                    x.ProductVariantId,
                    x.ProductName,
                    x.VariantName,
                    x.Sku,
                    x.Quantity,
                    new OrderPriceSnapshotDto(
                        x.UnitPriceSnapshot.Amount,
                        x.UnitPriceSnapshot.CurrencyCode,
                        x.UnitPriceSnapshot.CompareAtAmount,
                        x.UnitPriceSnapshot.PriceListId,
                        x.UnitPriceSnapshot.PriceEntryId),
                    x.LineSubtotalAmount,
                    x.LineDiscountAmount,
                    x.LineTaxAmount,
                    x.LineTotalAmount))
                .ToArray());
    }
}
