using Pricing.Contracts;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.ReadServices;

namespace Pricing.Infrastructure.IntegrationTests.ReadServices;

[TestClass]
public sealed class PriceCoverageReadServiceIntegrationTests
{
    [TestMethod]
    public async Task CheckCoverageAsync_WhenAllTargetsArePriced_ReturnsCovered()
    {
        var (connection, context) = await InfrastructureTestContextFactory.CreateAsync();
        await using var _ = connection;
        await using var __ = context;

        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"), isDefault: true);
        priceList.SetVariantPrice(productId, variantId, Money.Create(25m, "USD"));
        priceList.Activate();

        await context.PriceLists.AddAsync(priceList);
        await context.SaveChangesAsync();

        var readService = new PriceCoverageReadService(context);
        var result = await readService.CheckCoverageAsync(new CheckPriceCoverageRequest(
            storeId,
            new[] { new PriceCoverageTarget(productId, variantId) },
            "USD"));

        Assert.IsTrue(result.HasCoverage);
        Assert.HasCount(0, result.MissingTargets);
    }

    [TestMethod]
    public async Task CheckCoverageAsync_WhenTargetIsMissing_ReturnsMissingTarget()
    {
        var (connection, context) = await InfrastructureTestContextFactory.CreateAsync();
        await using var _ = connection;
        await using var __ = context;

        var storeId = Guid.NewGuid();
        var pricedProductId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"), isDefault: true);
        priceList.SetProductPrice(pricedProductId, Money.Create(25m, "USD"));
        priceList.Activate();

        await context.PriceLists.AddAsync(priceList);
        await context.SaveChangesAsync();

        var readService = new PriceCoverageReadService(context);
        var result = await readService.CheckCoverageAsync(new CheckPriceCoverageRequest(
            storeId,
            new[]
            {
                new PriceCoverageTarget(pricedProductId, null),
                new PriceCoverageTarget(missingProductId, null)
            },
            "USD"));

        Assert.IsFalse(result.HasCoverage);
        Assert.HasCount(1, result.MissingTargets);
        Assert.AreEqual(missingProductId, result.MissingTargets.Single().ProductId);
    }
}
