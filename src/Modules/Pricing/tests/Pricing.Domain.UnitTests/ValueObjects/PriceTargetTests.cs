using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.UnitTests.ValueObjects;

[TestClass]
public sealed class PriceTargetTests
{
    [TestMethod]
    public void ForProduct_WithValidProductId_CreatesProductTarget()
    {
        var productId = Guid.NewGuid();

        var target = PriceTarget.ForProduct(productId);

        Assert.AreEqual(productId, target.ProductId);
        Assert.IsNull(target.ProductVariantId);
    }

    [TestMethod]
    public void ForVariant_WithValidIds_CreatesVariantTarget()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var target = PriceTarget.ForVariant(productId, variantId);

        Assert.AreEqual(productId, target.ProductId);
        Assert.AreEqual(variantId, target.ProductVariantId);
    }

    [TestMethod]
    public void ForProduct_WithEmptyProductId_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PriceTarget.ForProduct(Guid.Empty));
    }

    [TestMethod]
    public void ForVariant_WithEmptyVariantId_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PriceTarget.ForVariant(Guid.NewGuid(), Guid.Empty));
    }
}
