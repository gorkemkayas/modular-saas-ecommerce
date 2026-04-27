namespace Order.Application.Orders.DTOs;

public sealed record OrderTotalsDto(
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal GrandTotalAmount);
