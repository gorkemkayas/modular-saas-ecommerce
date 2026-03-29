using Catalog.Application.Abstractions;
using Catalog.Application.Categories.Commands.ChangeCategoryParent;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.Application.UnitTests.Categories.Commands.ChangeCategoryParent
{
    [TestClass]
    public sealed class ChangeCategoryParentCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WhenParentIsDescendant_ThrowsCatalogValidationException()
        {
            var storeId = Guid.NewGuid();
            var category = Category.Create(storeId, "Shoes", Slug.Create("shoes"));
            var proposedParentId = Guid.NewGuid();

            var categoryRepository = new Mock<ICategoryRepository>();
            categoryRepository
                .Setup(x => x.GetByIdAsync(storeId, category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            categoryRepository
                .Setup(x => x.ExistsByIdAsync(storeId, proposedParentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            categoryRepository
                .Setup(x => x.IsDescendantOfAsync(storeId, proposedParentId, category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new ChangeCategoryParentCommandHandler(
                categoryRepository.Object,
                Mock.Of<IUnitOfWork>());

            var command = new ChangeCategoryParentCommand(storeId, category.Id, proposedParentId);

            await Assert.ThrowsExactlyAsync<CatalogValidationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
