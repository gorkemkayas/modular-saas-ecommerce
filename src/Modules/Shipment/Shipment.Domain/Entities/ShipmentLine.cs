using Shipment.Domain.Exceptions;

namespace Shipment.Domain.Entities;

public sealed class ShipmentLine
{
    private ShipmentLine()
    {
    }

    private ShipmentLine(
        Guid id,
        Guid shipmentId,
        Guid orderItemId,
        Guid productId,
        Guid? productVariantId,
        string productName,
        string? variantName,
        string? sku,
        int quantity)
    {
        if (shipmentId == Guid.Empty)
            throw new ShipmentDomainException("Shipment id is required.");

        if (orderItemId == Guid.Empty)
            throw new ShipmentDomainException("Order item id is required.");

        if (productId == Guid.Empty)
            throw new ShipmentDomainException("Product id is required.");

        if (quantity <= 0)
            throw new ShipmentDomainException("Shipment quantity must be greater than zero.");

        Id = id;
        ShipmentId = shipmentId;
        OrderItemId = orderItemId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = NormalizeRequired(productName, "Product name", 200);
        VariantName = NormalizeOptional(variantName, 200);
        Sku = NormalizeOptional(sku, 100);
        Quantity = quantity;
    }

    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string? VariantName { get; private set; }
    public string? Sku { get; private set; }
    public int Quantity { get; private set; }

    internal static ShipmentLine Create(
        Guid shipmentId,
        Guid orderItemId,
        Guid productId,
        Guid? productVariantId,
        string productName,
        string? variantName,
        string? sku,
        int quantity)
    {
        return new ShipmentLine(
            Guid.NewGuid(),
            shipmentId,
            orderItemId,
            productId,
            productVariantId,
            productName,
            variantName,
            sku,
            quantity);
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ShipmentDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new ShipmentDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new ShipmentDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
