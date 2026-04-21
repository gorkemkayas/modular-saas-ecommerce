using Pricing.Domain.Entities;
using Pricing.Domain.Exceptions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.UnitTests.Entities;

[TestClass]
public sealed class PriceEntryTests
{
    [TestMethod]
    public void Create_WithCompareAtPriceLowerThanPrice_ThrowsPricingDomainException()
    {
        Assert.ThrowsExactly<PricingDomainException>(() =>
            PriceEntry.Create(
                Guid.NewGuid(),
                PriceTarget.ForProduct(Guid.NewGuid()),
                Money.Create(100m, "TRY"),
                Money.Create(90m, "TRY")));
    }

    [TestMethod]
    public void Update_WithValidPrice_UpdatesAmount()
    {
        var entry = PriceEntry.Create(
            Guid.NewGuid(),
            PriceTarget.ForProduct(Guid.NewGuid()),
            Money.Create(100m, "TRY"));

        entry.Update(Money.Create(125m, "TRY"), Money.Create(150m, "TRY"));

        Assert.AreEqual(125m, entry.Price.Amount);
        Assert.AreEqual(150m, entry.CompareAtPrice!.Amount);
    }

    [TestMethod]
    public void Deactivate_WhenActive_MarksEntryInactive()
    {
        var entry = PriceEntry.Create(
            Guid.NewGuid(),
            PriceTarget.ForProduct(Guid.NewGuid()),
            Money.Create(100m, "TRY"));

        entry.Deactivate();

        Assert.IsFalse(entry.IsActive);
    }
}
