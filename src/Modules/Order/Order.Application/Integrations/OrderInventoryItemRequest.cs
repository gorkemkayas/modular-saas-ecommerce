namespace Order.Application.Integrations;

public sealed record OrderInventoryItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
