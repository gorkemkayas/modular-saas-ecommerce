using Moq;
using Store.Application.Stores.Queries.ListPublishedStorefrontStores;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using StoreEntity = Store.Domain.Stores.Store;

namespace Store.Application.Stores.Queries.ListPublishedStorefrontStores.UnitTests
{
    [TestClass]
    public sealed class ListPublishedStorefrontStoresQueryHandlerTests
    {
        [TestMethod]
        public async Task Handle_PublishedStoresExist_ReturnsStoreSummaries()
        {
            var store = StoreEntity.Create(
                Guid.NewGuid(),
                "North Studio",
                Slug.Create("north-studio"),
                logoUrl: "https://example.com/logo.png");
            store.Publish();

            var repository = new Mock<IStoreRepository>();
            repository
                .Setup(x => x.ListPublishedAsync(16, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { store });

            var handler = new ListPublishedStorefrontStoresQueryHandler(repository.Object);

            var result = await handler.Handle(
                new ListPublishedStorefrontStoresQuery(),
                CancellationToken.None);

            var summary = result.Single();

            Assert.AreEqual(store.TenantId, summary.TenantId);
            Assert.AreEqual(store.Name, summary.Name);
            Assert.AreEqual(store.Slug.Value, summary.Slug);
            Assert.AreEqual(store.LogoUrl, summary.LogoUrl);
        }

        [TestMethod]
        public async Task Handle_InvalidLimit_UsesDefaultLimit()
        {
            var repository = new Mock<IStoreRepository>();
            repository
                .Setup(x => x.ListPublishedAsync(16, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<StoreEntity>());

            var handler = new ListPublishedStorefrontStoresQueryHandler(repository.Object);

            await handler.Handle(
                new ListPublishedStorefrontStoresQuery(0),
                CancellationToken.None);

            repository.Verify(
                x => x.ListPublishedAsync(16, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Handle_LimitAboveMaximum_ClampsLimit()
        {
            var repository = new Mock<IStoreRepository>();
            repository
                .Setup(x => x.ListPublishedAsync(32, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<StoreEntity>());

            var handler = new ListPublishedStorefrontStoresQueryHandler(repository.Object);

            await handler.Handle(
                new ListPublishedStorefrontStoresQuery(100),
                CancellationToken.None);

            repository.Verify(
                x => x.ListPublishedAsync(32, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
