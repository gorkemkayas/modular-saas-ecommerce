using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Application.Integrations;
using Inventory.Application.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Inventory.Application.UnitTests.InventoryItems.Commands.CreateInventoryItem;

[TestClass]
public sealed class CreateInventoryItemCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenInventoryItemAlreadyExists_ThrowsDuplicateInventoryItemException()
    {
        var repository = new Mock<IInventoryItemRepository>();
        var catalogService = new Mock<IInventoryCatalogService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.ExistsBySellableItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateInventoryItemCommandHandler(
            repository.Object,
            catalogService.Object,
            unitOfWork.Object,
            NullLogger<CreateInventoryItemCommandHandler>.Instance);

        await Assert.ThrowsExactlyAsync<DuplicateInventoryItemException>(() =>
            handler.Handle(
                new CreateInventoryItemCommand(Guid.NewGuid(), Guid.NewGuid(), null, 5, 2),
                CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_WhenCatalogItemIsValid_PersistsInventoryItem()
    {
        var repository = new Mock<IInventoryItemRepository>();
        var catalogService = new Mock<IInventoryCatalogService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.ExistsBySellableItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        catalogService
            .Setup(x => x.GetSellableItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid storeId, Guid productId, Guid? variantId, CancellationToken _) =>
                new InventorySellableItem(productId, variantId, "Phone", "Black", "SKU-1"));

        var handler = new CreateInventoryItemCommandHandler(
            repository.Object,
            catalogService.Object,
            unitOfWork.Object,
            NullLogger<CreateInventoryItemCommandHandler>.Instance);

        var inventoryItemId = await handler.Handle(
            new CreateInventoryItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 8, 3),
            CancellationToken.None);

        Assert.AreNotEqual(Guid.Empty, inventoryItemId);
        repository.Verify(x => x.AddAsync(It.IsAny<Inventory.Domain.Entities.InventoryItem>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
