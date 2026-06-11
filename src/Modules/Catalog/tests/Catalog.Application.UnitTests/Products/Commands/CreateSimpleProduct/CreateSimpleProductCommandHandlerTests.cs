using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands.CreateSimpleProduct;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Subscription.Contracts;

namespace Catalog.Application.UnitTests.Products.Commands.CreateSimpleProduct
{
    [TestClass]
    public sealed class CreateSimpleProductCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WhenSlugAlreadyExists_ThrowsDuplicateProductSlugException()
        {
            var storeId = Guid.NewGuid();
            var command = new CreateSimpleProductCommand(
                storeId,
                "Basic Tee",
                "basic-tee",
                "TEE-001",
                null,
                null,
                null,
                null);

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.ExistsBySlugAsync(
                    storeId,
                    It.Is<Slug>(slug => slug.Value == "basic-tee"),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateSimpleProductCommandHandler(
                productRepository.Object,
                Mock.Of<IBrandRepository>(),
                Mock.Of<ICategoryRepository>(),
                Mock.Of<ISubscriptionModuleApi>(),
                Mock.Of<IUnitOfWork>(),
                Mock.Of<ILogger<CreateSimpleProductCommandHandler>>());

            await Assert.ThrowsExactlyAsync<DuplicateProductSlugException>(() => handler.Handle(command, CancellationToken.None));

            productRepository.Verify(
                x => x.ExistsBySkuAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Sku>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Handle_WithValidCommand_AddsProductAndReturnsId()
        {
            var storeId = Guid.NewGuid();
            var command = new CreateSimpleProductCommand(
                storeId,
                "Basic Tee",
                "basic-tee",
                "TEE-001",
                "Short",
                "Long description",
                null,
                new[] { Guid.NewGuid() });

            Catalog.Domain.Entities.Product? addedProduct = null;

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.ExistsBySlugAsync(storeId, It.IsAny<Slug>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            productRepository
                .Setup(x => x.ExistsBySkuAsync(storeId, It.IsAny<Sku>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            productRepository
                .Setup(x => x.CountNonArchivedByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            productRepository
                .Setup(x => x.AddAsync(It.IsAny<Catalog.Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Callback<Catalog.Domain.Entities.Product, CancellationToken>((product, _) => addedProduct = product)
                .Returns(Task.CompletedTask);

            var categoryRepository = new Mock<ICategoryRepository>();
            categoryRepository
                .Setup(x => x.ExistsByIdAsync(storeId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var subscriptionModuleApi = new Mock<ISubscriptionModuleApi>();
            subscriptionModuleApi
                .Setup(x => x.GetQuotaAsync(
                    It.Is<QuotaRequest>(request =>
                        request.TenantId == storeId &&
                        request.QuotaKey == SubscriptionQuotaKeys.CatalogProducts),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QuotaResult(storeId, SubscriptionQuotaKeys.CatalogProducts, 10));

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateSimpleProductCommandHandler(
                productRepository.Object,
                Mock.Of<IBrandRepository>(),
                categoryRepository.Object,
                subscriptionModuleApi.Object,
                unitOfWork.Object,
                Mock.Of<ILogger<CreateSimpleProductCommandHandler>>());

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.IsNotNull(addedProduct);
            Assert.AreEqual(result, addedProduct.Id);
            Assert.AreEqual(storeId, addedProduct.StoreId);
            Assert.AreEqual("basic-tee", addedProduct.Slug.Value);
            Assert.AreEqual("TEE-001", addedProduct.Sku!.Value);
            Assert.HasCount(1, addedProduct.Categories);

            unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_WhenProductQuotaIsReached_ThrowsCatalogQuotaExceededException()
        {
            var storeId = Guid.NewGuid();
            var command = new CreateSimpleProductCommand(
                storeId,
                "Basic Tee",
                "basic-tee",
                "TEE-001",
                null,
                null,
                null,
                null);

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.ExistsBySlugAsync(storeId, It.IsAny<Slug>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            productRepository
                .Setup(x => x.ExistsBySkuAsync(storeId, It.IsAny<Sku>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            productRepository
                .Setup(x => x.CountNonArchivedByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(50);

            var subscriptionModuleApi = new Mock<ISubscriptionModuleApi>();
            subscriptionModuleApi
                .Setup(x => x.GetQuotaAsync(
                    It.Is<QuotaRequest>(request =>
                        request.TenantId == storeId &&
                        request.QuotaKey == SubscriptionQuotaKeys.CatalogProducts),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QuotaResult(storeId, SubscriptionQuotaKeys.CatalogProducts, 50));

            var handler = new CreateSimpleProductCommandHandler(
                productRepository.Object,
                Mock.Of<IBrandRepository>(),
                Mock.Of<ICategoryRepository>(),
                subscriptionModuleApi.Object,
                Mock.Of<IUnitOfWork>(),
                Mock.Of<ILogger<CreateSimpleProductCommandHandler>>());

            var exception = await Assert.ThrowsExactlyAsync<CatalogQuotaExceededException>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.AreEqual(storeId, exception.StoreId);
            Assert.AreEqual(SubscriptionQuotaKeys.CatalogProducts, exception.QuotaKey);
            Assert.AreEqual(50, exception.CurrentCount);
            Assert.AreEqual(50, exception.Limit);

            productRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Catalog.Domain.Entities.Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
