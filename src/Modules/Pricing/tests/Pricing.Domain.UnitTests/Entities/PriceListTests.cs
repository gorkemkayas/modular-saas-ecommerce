using Pricing.Domain.Entities;
using Pricing.Domain.Enums;
using Pricing.Domain.Exceptions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.UnitTests.Entities;

[TestClass]
public sealed class PriceListTests
{
    [TestMethod]
    public void Create_WithValidArguments_CreatesDraftPriceList()
    {
        var storeId = Guid.NewGuid();

        var priceList = PriceList.Create(storeId, "Main TRY", Currency.Create("TRY"), priority: 10, isDefault: true);

        Assert.AreNotEqual(Guid.Empty, priceList.Id);
        Assert.AreEqual(storeId, priceList.StoreId);
        Assert.AreEqual("Main TRY", priceList.Name);
        Assert.AreEqual("TRY", priceList.Currency.Code);
        Assert.AreEqual(10, priceList.Priority);
        Assert.IsTrue(priceList.IsDefault);
        Assert.AreEqual(PriceListStatus.Draft, priceList.Status);
    }

    [TestMethod]
    public void SetProductPrice_WhenEntryDoesNotExist_AddsEntry()
    {
        var priceList = CreatePriceList();
        var productId = Guid.NewGuid();

        priceList.SetProductPrice(productId, Money.Create(100m, "TRY"), Money.Create(120m, "TRY"));

        Assert.HasCount(1, priceList.Entries);

        var entry = priceList.Entries.Single();
        Assert.AreEqual(productId, entry.Target.ProductId);
        Assert.IsNull(entry.Target.ProductVariantId);
        Assert.AreEqual(100m, entry.Price.Amount);
        Assert.AreEqual(120m, entry.CompareAtPrice!.Amount);
    }

    [TestMethod]
    public void SetProductPrice_WhenEntryExists_UpdatesExistingEntry()
    {
        var priceList = CreatePriceList();
        var productId = Guid.NewGuid();

        priceList.SetProductPrice(productId, Money.Create(100m, "TRY"));
        var entryId = priceList.Entries.Single().Id;

        priceList.SetProductPrice(productId, Money.Create(150m, "TRY"));

        Assert.HasCount(1, priceList.Entries);
        Assert.AreEqual(entryId, priceList.Entries.Single().Id);
        Assert.AreEqual(150m, priceList.Entries.Single().Price.Amount);
    }

    [TestMethod]
    public void SetVariantPrice_WithDifferentCurrency_ThrowsPricingDomainException()
    {
        var priceList = CreatePriceList();

        Assert.ThrowsExactly<PricingDomainException>(() =>
            priceList.SetVariantPrice(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(100m, "USD")));
    }

    [TestMethod]
    public void RemovePrice_WhenEntryExists_RemovesEntry()
    {
        var priceList = CreatePriceList();
        var productId = Guid.NewGuid();

        priceList.SetProductPrice(productId, Money.Create(100m, "TRY"));
        priceList.RemovePrice(productId);

        Assert.HasCount(0, priceList.Entries);
    }

    [TestMethod]
    public void Archive_PreventsFurtherMutation()
    {
        var priceList = CreatePriceList();
        priceList.Archive();

        Assert.AreEqual(PriceListStatus.Archived, priceList.Status);
        Assert.IsFalse(priceList.IsDefault);

        Assert.ThrowsExactly<PricingDomainException>(() =>
            priceList.SetProductPrice(Guid.NewGuid(), Money.Create(100m, "TRY")));
    }

    [TestMethod]
    public void ActivateAndDeactivate_ChangeStatus()
    {
        var priceList = CreatePriceList();

        priceList.Activate();
        Assert.AreEqual(PriceListStatus.Active, priceList.Status);

        priceList.Deactivate();
        Assert.AreEqual(PriceListStatus.Inactive, priceList.Status);
    }

    private static PriceList CreatePriceList()
    {
        return PriceList.Create(Guid.NewGuid(), "Main TRY", Currency.Create("TRY"), isDefault: true);
    }
}
