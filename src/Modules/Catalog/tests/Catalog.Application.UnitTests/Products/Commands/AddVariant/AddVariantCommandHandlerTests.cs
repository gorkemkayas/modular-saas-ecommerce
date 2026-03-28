using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands.AddVariant;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.Application.UnitTests.Products.Commands.AddVariant
{
    [TestClass]
    public sealed class AddVariantCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WhenAttributeIsNotVariantDefining_ThrowsCatalogValidationException()
        {
            var storeId = Guid.NewGuid();
            var product = Product.CreateVariant(storeId, "Sneaker", Slug.Create("sneaker"));
            var attributeId = Guid.NewGuid();

            var productRepository = new Mock<IProductRepository>();
            productRepository
                .Setup(x => x.GetByIdAsync(storeId, product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            productRepository
                .Setup(x => x.ExistsBySkuAsync(storeId, It.IsAny<Sku>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var attributeDefinitionRepository = new Mock<IAttributeDefinitionRepository>();
            attributeDefinitionRepository
                .Setup(x => x.GetByIdsAsync(
                    storeId,
                    It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(attributeId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    AttributeDefinition.Create(
                        storeId,
                        "Color",
                        AttributeCode.Create("color"),
                        AttributeDataType.String,
                        isVariantDefining: false)
                });

            var handler = new AddVariantCommandHandler(
                productRepository.Object,
                attributeDefinitionRepository.Object,
                Mock.Of<IUnitOfWork>(),
                Mock.Of<ILogger<AddVariantCommandHandler>>());

            var command = new AddVariantCommand(
                storeId,
                product.Id,
                "SN-RED-42",
                "Red / 42",
                0,
                new[] { new VariantAttributeValueInput(attributeId, "Red") });

            await Assert.ThrowsExactlyAsync<CatalogValidationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
