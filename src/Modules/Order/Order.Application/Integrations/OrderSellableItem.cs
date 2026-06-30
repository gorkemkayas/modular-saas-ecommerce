namespace Order.Application.Integrations;

public sealed record OrderSellableItem(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    string? ImageUrl = null);
