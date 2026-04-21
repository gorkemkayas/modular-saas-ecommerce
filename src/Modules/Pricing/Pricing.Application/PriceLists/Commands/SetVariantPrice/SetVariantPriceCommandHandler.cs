using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Application.Integrations;
using Pricing.Domain.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.PriceLists.Commands.SetVariantPrice;

public sealed class SetVariantPriceCommandHandler : IRequestHandler<SetVariantPriceCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly ICatalogSellableItemValidator _catalogSellableItemValidator;
    private readonly IUnitOfWork _unitOfWork;

    public SetVariantPriceCommandHandler(
        IPriceListRepository priceListRepository,
        ICatalogSellableItemValidator catalogSellableItemValidator,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _catalogSellableItemValidator = catalogSellableItemValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetVariantPriceCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        var validation = await _catalogSellableItemValidator.ValidateAsync(
            command.StoreId,
            command.ProductId,
            command.ProductVariantId,
            cancellationToken);

        if (!validation.ProductExists)
            throw new InvalidPriceTargetException("Catalog product was not found.");

        if (!validation.VariantExists || !validation.VariantBelongsToProduct)
            throw new InvalidPriceTargetException("Catalog product variant was not found.");

        if (validation.ProductType != CatalogSellableItemType.Variant)
            throw new InvalidPriceTargetException("Variant pricing is allowed only for variant products.");

        if (!validation.VariantIsActive)
            throw new InvalidPriceTargetException("Inactive product variants cannot be priced.");

        var price = Money.Create(command.Amount, priceList.Currency);
        var compareAt = command.CompareAtAmount.HasValue
            ? Money.Create(command.CompareAtAmount.Value, priceList.Currency)
            : null;

        priceList.SetVariantPrice(command.ProductId, command.ProductVariantId, price, compareAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
