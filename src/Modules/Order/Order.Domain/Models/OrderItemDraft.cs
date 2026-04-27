using Order.Domain.ValueObjects;

namespace Order.Domain.Models;

public sealed record OrderItemDraft(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity,
    OrderPriceSnapshot UnitPriceSnapshot);
