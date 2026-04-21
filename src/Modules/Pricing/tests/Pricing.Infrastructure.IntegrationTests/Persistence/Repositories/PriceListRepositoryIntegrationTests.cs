using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence.Repositories;

namespace Pricing.Infrastructure.IntegrationTests.Persistence.Repositories;

[TestClass]
public sealed class PriceListRepositoryIntegrationTests
{
    [TestMethod]
    public async Task AddAsync_AndGetByIdAsync_ReturnsPriceListWithEntries()
    {
        var (connection, context) = await InfrastructureTestContextFactory.CreateAsync();
        await using var _ = connection;
        await using var __ = context;

        var repository = new PriceListRepository(context);
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));
        priceList.SetProductPrice(productId, Money.Create(25m, "USD"));

        await repository.AddAsync(priceList);
        await context.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(storeId, priceList.Id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(priceList.Id, loaded.Id);
        Assert.HasCount(1, loaded.Entries);
        Assert.AreEqual(productId, loaded.Entries.Single().Target.ProductId);
    }
}
