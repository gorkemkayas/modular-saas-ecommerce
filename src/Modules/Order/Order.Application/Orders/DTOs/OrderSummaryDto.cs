using Order.Domain.Enums;

namespace Order.Application.Orders.DTOs;

public sealed record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    FulfillmentStatus FulfillmentStatus,
    string CurrencyCode,
    string? ShippingCarrierName,
    int ItemCount,
    decimal GrandTotalAmount,
    DateTime PlacedAtUtc);
