namespace Inventory.Application.Integrations;

public sealed record InventorySellableItem(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string Sku);
