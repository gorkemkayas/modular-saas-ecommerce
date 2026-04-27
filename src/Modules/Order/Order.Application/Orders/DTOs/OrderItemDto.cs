namespace Order.Application.Orders.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity,
    OrderPriceSnapshotDto UnitPriceSnapshot,
    decimal LineSubtotalAmount,
    decimal LineDiscountAmount,
    decimal LineTaxAmount,
    decimal LineTotalAmount);
