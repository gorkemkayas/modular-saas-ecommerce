using Order.Domain.Enums;

namespace Order.Application.Orders.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    FulfillmentStatus FulfillmentStatus,
    string CurrencyCode,
    OrderCustomerSnapshotDto Customer,
    OrderAddressSnapshotDto BillingAddress,
    OrderAddressSnapshotDto ShippingAddress,
    OrderTotalsDto Totals,
    DateTime PlacedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancellationReason,
    string? ReservationReference,
    string? PaymentReference,
    string? ShipmentReference,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<OrderItemDto> Items);
