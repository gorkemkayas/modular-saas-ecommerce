using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class PriceTarget : ValueObject
{
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }

    private PriceTarget()
    {
    }

    private PriceTarget(Guid productId, Guid? productVariantId)
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
    }

    public static PriceTarget ForProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

        return new PriceTarget(productId, null);
    }

    public static PriceTarget ForVariant(Guid productId, Guid productVariantId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

        if (productVariantId == Guid.Empty)
            throw new ArgumentException("ProductVariantId cannot be empty.", nameof(productVariantId));

        return new PriceTarget(productId, productVariantId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductVariantId;
    }
}
