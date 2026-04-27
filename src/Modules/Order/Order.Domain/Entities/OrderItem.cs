using Order.Domain.Exceptions;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        Guid? productVariantId,
        string productName,
        string? variantName,
        string? sku,
        int quantity,
        OrderPriceSnapshot unitPriceSnapshot)
    {
        if (orderId == Guid.Empty)
            throw new OrderDomainException("Order id is required.");

        if (productId == Guid.Empty)
            throw new OrderDomainException("Product id is required.");

        if (quantity <= 0)
            throw new OrderDomainException("Order item quantity must be greater than zero.");

        Id = id;
        OrderId = orderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = NormalizeRequired(productName, "Product name");
        VariantName = NormalizeOptional(variantName, 200);
        Sku = NormalizeOptional(sku, 100);
        Quantity = quantity;
        UnitPriceSnapshot = unitPriceSnapshot ?? throw new ArgumentNullException(nameof(unitPriceSnapshot));
        LineSubtotalAmount = quantity * unitPriceSnapshot.Amount;
        LineDiscountAmount = 0m;
        LineTaxAmount = 0m;
        LineTotalAmount = LineSubtotalAmount;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string? VariantName { get; private set; }
    public string? Sku { get; private set; }
    public int Quantity { get; private set; }
    public OrderPriceSnapshot UnitPriceSnapshot { get; private set; } = default!;
    public decimal LineSubtotalAmount { get; private set; }
    public decimal LineDiscountAmount { get; private set; }
    public decimal LineTaxAmount { get; private set; }
    public decimal LineTotalAmount { get; private set; }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        Guid? productVariantId,
        string productName,
        string? variantName,
        string? sku,
        int quantity,
        OrderPriceSnapshot unitPriceSnapshot)
    {
        return new OrderItem(
            Guid.NewGuid(),
            orderId,
            productId,
            productVariantId,
            productName,
            variantName,
            sku,
            quantity,
            unitPriceSnapshot);
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException($"{fieldName} is required.");

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new OrderDomainException("Order item value is too long.");

        return normalized;
    }
}
