using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands.PublishProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.Application.UnitTests.Products.Commands.PublishProduct
{
    [TestClass]
    public sealed class PublishProductCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WhenRequiredPriceExists_PublishesProduct()
        {
            var storeId = Guid.NewGuid();
            var product = CreatePublishableSimpleProduct(storeId);

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.GetByIdAsync(storeId, product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var pricingChecker = new Mock<IProductPricingAvailabilityChecker>();
            pricingChecker
                .Setup(x => x.HasRequiredPricesAsync(
                    storeId,
                    It.Is<IReadOnlyCollection<ProductPricingAvailabilityTarget>>(targets =>
                        targets.Count == 1 &&
                        targets.Single().ProductId == product.Id &&
                        targets.Single().ProductVariantId == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            var handler = new PublishProductCommandHandler(
                productRepository.Object,
                pricingChecker.Object,
                unitOfWork.Object);

            await handler.Handle(new PublishProductCommand(storeId, product.Id), CancellationToken.None);

            Assert.IsTrue(product.IsPublished);
            unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_WhenRequiredPriceIsMissing_ThrowsProductPricingRequiredException()
        {
            var storeId = Guid.NewGuid();
            var product = CreatePublishableSimpleProduct(storeId);

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.GetByIdAsync(storeId, product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var pricingChecker = new Mock<IProductPricingAvailabilityChecker>();
            pricingChecker
                .Setup(x => x.HasRequiredPricesAsync(
                    storeId,
                    It.IsAny<IReadOnlyCollection<ProductPricingAvailabilityTarget>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = new PublishProductCommandHandler(
                productRepository.Object,
                pricingChecker.Object,
                Mock.Of<IUnitOfWork>());

            await Assert.ThrowsExactlyAsync<ProductPricingRequiredException>(() =>
                handler.Handle(new PublishProductCommand(storeId, product.Id), CancellationToken.None));

            Assert.IsFalse(product.IsPublished);
        }

        private static Product CreatePublishableSimpleProduct(Guid storeId)
        {
            var product = Product.CreateSimple(
                storeId,
                "Keyboard",
                Slug.Create("keyboard"),
                Sku.Create("KEY-001"));

            product.AssignCategories(new[] { Guid.NewGuid() });

            return product;
        }
    }
}
