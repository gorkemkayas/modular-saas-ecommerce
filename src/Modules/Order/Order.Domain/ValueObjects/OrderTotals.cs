using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class OrderTotals : ValueObject
{
    private OrderTotals()
    {
    }

    private OrderTotals(decimal subtotalAmount, decimal discountAmount, decimal shippingAmount, decimal taxAmount, decimal grandTotalAmount)
    {
        SubtotalAmount = subtotalAmount;
        DiscountAmount = discountAmount;
        ShippingAmount = shippingAmount;
        TaxAmount = taxAmount;
        GrandTotalAmount = grandTotalAmount;
    }

    public decimal SubtotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal GrandTotalAmount { get; private set; }

    public static OrderTotals Create(decimal subtotalAmount, decimal discountAmount = 0m, decimal shippingAmount = 0m, decimal taxAmount = 0m)
    {
        if (subtotalAmount < 0 || discountAmount < 0 || shippingAmount < 0 || taxAmount < 0)
            throw new OrderDomainException("Order totals cannot contain negative amounts.");

        var grandTotal = subtotalAmount - discountAmount + shippingAmount + taxAmount;

        if (grandTotal < 0)
            throw new OrderDomainException("Grand total cannot be negative.");

        return new OrderTotals(subtotalAmount, discountAmount, shippingAmount, taxAmount, grandTotal);
    }

    public static OrderTotals FromItems(IEnumerable<Entities.OrderItem> items, decimal discountAmount = 0m, decimal shippingAmount = 0m, decimal taxAmount = 0m)
    {
        var subtotal = items.Sum(x => x.LineSubtotalAmount);
        return Create(subtotal, discountAmount, shippingAmount, taxAmount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SubtotalAmount;
        yield return DiscountAmount;
        yield return ShippingAmount;
        yield return TaxAmount;
        yield return GrandTotalAmount;
    }
}
