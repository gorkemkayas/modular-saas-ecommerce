using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.ReadServices;

namespace Pricing.Infrastructure.IntegrationTests.ReadServices;

[TestClass]
public sealed class PriceResolutionReadServiceIntegrationTests
{
    [TestMethod]
    public async Task GetResolvedPriceAsync_WhenDefaultActiveListHasMatchingEntry_ReturnsPrice()
    {
        var (connection, context) = await InfrastructureTestContextFactory.CreateAsync();
        await using var _ = connection;
        await using var __ = context;

        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"), isDefault: true);
        priceList.SetProductPrice(productId, Money.Create(25m, "USD"), Money.Create(30m, "USD"));
        priceList.Activate();

        await context.PriceLists.AddAsync(priceList);
        await context.SaveChangesAsync();

        var readService = new PriceResolutionReadService(context);
        var resolved = await readService.GetResolvedPriceAsync(storeId, productId, null, "USD");

        Assert.IsNotNull(resolved);
        Assert.AreEqual(25m, resolved.Amount);
        Assert.AreEqual(30m, resolved.CompareAtAmount);
        Assert.AreEqual(priceList.Id, resolved.PriceListId);
    }

    [TestMethod]
    public async Task GetResolvedPriceAsync_WhenDefaultListIsInactive_ReturnsNull()
    {
        var (connection, context) = await InfrastructureTestContextFactory.CreateAsync();
        await using var _ = connection;
        await using var __ = context;

        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"), isDefault: true);
        priceList.SetProductPrice(productId, Money.Create(25m, "USD"));

        await context.PriceLists.AddAsync(priceList);
        await context.SaveChangesAsync();

        var readService = new PriceResolutionReadService(context);
        var resolved = await readService.GetResolvedPriceAsync(storeId, productId, null, "USD");

        Assert.IsNull(resolved);
    }
}
