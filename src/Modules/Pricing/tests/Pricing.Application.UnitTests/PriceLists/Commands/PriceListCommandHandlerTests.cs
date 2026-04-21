using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Application.Integrations;
using Pricing.Application.PriceLists.Commands.ActivatePriceEntry;
using Pricing.Application.PriceLists.Commands.CreatePriceList;
using Pricing.Application.PriceLists.Commands.DeactivatePriceEntry;
using Pricing.Application.PriceLists.Commands.RemovePrice;
using Pricing.Application.PriceLists.Commands.SetDefaultPriceList;
using Pricing.Application.PriceLists.Commands.SetProductPrice;
using Pricing.Application.PriceLists.Commands.SetVariantPrice;
using Pricing.Domain.Entities;
using Pricing.Domain.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UnitTests.PriceLists.Commands;

[TestClass]
public sealed class PriceListCommandHandlerTests
{
    [TestMethod]
    public async Task CreatePriceList_WhenDefaultAlreadyExists_ThrowsDuplicateDefaultPriceListException()
    {
        var storeId = Guid.NewGuid();
        var repository = new Mock<IPriceListRepository>();
        repository
            .Setup(x => x.ExistsDefaultActiveListAsync(
                storeId,
                Currency.Create("USD"),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreatePriceListCommandHandler(
            repository.Object,
            Mock.Of<IUnitOfWork>(),
            NullLogger<CreatePriceListCommandHandler>.Instance);

        await Assert.ThrowsExactlyAsync<DuplicateDefaultPriceListException>(() =>
            handler.Handle(new CreatePriceListCommand(storeId, "Default", "USD", 0, true), CancellationToken.None));
    }

    [TestMethod]
    public async Task CreatePriceList_WhenValid_AddsPriceListAndSaves()
    {
        var storeId = Guid.NewGuid();
        var repository = new Mock<IPriceListRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreatePriceListCommandHandler(
            repository.Object,
            unitOfWork.Object,
            NullLogger<CreatePriceListCommandHandler>.Instance);

        var priceListId = await handler.Handle(
            new CreatePriceListCommand(storeId, "Default", "USD", 10, false),
            CancellationToken.None);

        Assert.AreNotEqual(Guid.Empty, priceListId);
        repository.Verify(x => x.AddAsync(
            It.Is<PriceList>(priceList =>
                priceList.Id == priceListId &&
                priceList.StoreId == storeId &&
                priceList.Currency == Currency.Create("USD") &&
                priceList.Priority == 10),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SetProductPrice_WhenSimpleProductIsValid_SetsPriceAndSaves()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));

        var repository = CreateRepositoryReturning(priceList);
        var validator = new Mock<ICatalogSellableItemValidator>();
        validator
            .Setup(x => x.ValidateAsync(storeId, productId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SellableItemValidationResult(
                ProductExists: true,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: CatalogSellableItemType.Simple));

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new SetProductPriceCommandHandler(repository.Object, validator.Object, unitOfWork.Object);

        await handler.Handle(new SetProductPriceCommand(storeId, priceList.Id, productId, 99.99m, null), CancellationToken.None);

        var entry = priceList.Entries.Single();
        Assert.AreEqual(productId, entry.Target.ProductId);
        Assert.IsNull(entry.Target.ProductVariantId);
        Assert.AreEqual(99.99m, entry.Price.Amount);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SetProductPrice_WhenProductIsVariant_ThrowsInvalidPriceTargetException()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));

        var repository = CreateRepositoryReturning(priceList);
        var validator = new Mock<ICatalogSellableItemValidator>();
        validator
            .Setup(x => x.ValidateAsync(storeId, productId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SellableItemValidationResult(
                ProductExists: true,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: CatalogSellableItemType.Variant));

        var handler = new SetProductPriceCommandHandler(repository.Object, validator.Object, Mock.Of<IUnitOfWork>());

        await Assert.ThrowsExactlyAsync<InvalidPriceTargetException>(() =>
            handler.Handle(new SetProductPriceCommand(storeId, priceList.Id, productId, 99.99m, null), CancellationToken.None));
    }

    [TestMethod]
    public async Task SetVariantPrice_WhenVariantIsValid_SetsPriceAndSaves()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));

        var repository = CreateRepositoryReturning(priceList);
        var validator = new Mock<ICatalogSellableItemValidator>();
        validator
            .Setup(x => x.ValidateAsync(storeId, productId, variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SellableItemValidationResult(
                ProductExists: true,
                VariantExists: true,
                VariantBelongsToProduct: true,
                VariantIsActive: true,
                ProductType: CatalogSellableItemType.Variant));

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new SetVariantPriceCommandHandler(repository.Object, validator.Object, unitOfWork.Object);

        await handler.Handle(new SetVariantPriceCommand(storeId, priceList.Id, productId, variantId, 149.99m, 199.99m), CancellationToken.None);

        var entry = priceList.Entries.Single();
        Assert.AreEqual(productId, entry.Target.ProductId);
        Assert.AreEqual(variantId, entry.Target.ProductVariantId);
        Assert.AreEqual(149.99m, entry.Price.Amount);
        Assert.AreEqual(199.99m, entry.CompareAtPrice?.Amount);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SetVariantPrice_WhenVariantIsInactive_ThrowsInvalidPriceTargetException()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));

        var repository = CreateRepositoryReturning(priceList);
        var validator = new Mock<ICatalogSellableItemValidator>();
        validator
            .Setup(x => x.ValidateAsync(storeId, productId, variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SellableItemValidationResult(
                ProductExists: true,
                VariantExists: true,
                VariantBelongsToProduct: true,
                VariantIsActive: false,
                ProductType: CatalogSellableItemType.Variant));

        var handler = new SetVariantPriceCommandHandler(repository.Object, validator.Object, Mock.Of<IUnitOfWork>());

        await Assert.ThrowsExactlyAsync<InvalidPriceTargetException>(() =>
            handler.Handle(new SetVariantPriceCommand(storeId, priceList.Id, productId, variantId, 149.99m, null), CancellationToken.None));
    }

    [TestMethod]
    public async Task SetDefaultPriceList_WhenExistingDefaultExists_UnmarksExistingDefault()
    {
        var storeId = Guid.NewGuid();
        var currency = Currency.Create("USD");
        var existingDefault = PriceList.Create(storeId, "Current", currency, isDefault: true);
        var target = PriceList.Create(storeId, "Next", currency);

        var repository = CreateRepositoryReturning(target);
        repository
            .Setup(x => x.GetDefaultByStoreAndCurrencyAsync(storeId, currency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDefault);

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new SetDefaultPriceListCommandHandler(repository.Object, unitOfWork.Object);

        await handler.Handle(new SetDefaultPriceListCommand(storeId, target.Id), CancellationToken.None);

        Assert.IsFalse(existingDefault.IsDefault);
        Assert.IsTrue(target.IsDefault);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RemovePrice_WhenEntryExists_RemovesEntryAndSaves()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));
        priceList.SetProductPrice(productId, Money.Create(10m, "USD"));

        var repository = CreateRepositoryReturning(priceList);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RemovePriceCommandHandler(repository.Object, unitOfWork.Object);

        await handler.Handle(new RemovePriceCommand(storeId, priceList.Id, productId, null), CancellationToken.None);

        Assert.HasCount(0, priceList.Entries);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeactivatePriceEntry_WhenEntryExists_DeactivatesEntryAndSaves()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));
        priceList.SetProductPrice(productId, Money.Create(10m, "USD"));
        var entryId = priceList.Entries.Single().Id;

        var repository = CreateRepositoryReturning(priceList);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new DeactivatePriceEntryCommandHandler(repository.Object, unitOfWork.Object);

        await handler.Handle(new DeactivatePriceEntryCommand(storeId, priceList.Id, entryId), CancellationToken.None);

        Assert.IsFalse(priceList.Entries.Single().IsActive);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ActivatePriceEntry_WhenEntryExists_ActivatesEntryAndSaves()
    {
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var priceList = PriceList.Create(storeId, "Default", Currency.Create("USD"));
        priceList.SetProductPrice(productId, Money.Create(10m, "USD"));
        var entryId = priceList.Entries.Single().Id;
        priceList.DeactivatePriceEntry(entryId);

        var repository = CreateRepositoryReturning(priceList);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new ActivatePriceEntryCommandHandler(repository.Object, unitOfWork.Object);

        await handler.Handle(new ActivatePriceEntryCommand(storeId, priceList.Id, entryId), CancellationToken.None);

        Assert.IsTrue(priceList.Entries.Single().IsActive);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<IPriceListRepository> CreateRepositoryReturning(PriceList priceList)
    {
        var repository = new Mock<IPriceListRepository>();
        repository
            .Setup(x => x.GetByIdAsync(priceList.StoreId, priceList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        return repository;
    }
}
