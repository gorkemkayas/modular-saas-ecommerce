using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Application.Integrations;
using Pricing.Domain.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.PriceLists.Commands.SetProductPrice;

public sealed class SetProductPriceCommandHandler : IRequestHandler<SetProductPriceCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly ICatalogSellableItemValidator _catalogSellableItemValidator;
    private readonly IUnitOfWork _unitOfWork;

    public SetProductPriceCommandHandler(
        IPriceListRepository priceListRepository,
        ICatalogSellableItemValidator catalogSellableItemValidator,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _catalogSellableItemValidator = catalogSellableItemValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetProductPriceCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        var validation = await _catalogSellableItemValidator.ValidateAsync(
            command.StoreId,
            command.ProductId,
            productVariantId: null,
            cancellationToken);

        if (!validation.ProductExists)
            throw new InvalidPriceTargetException("Catalog product was not found.");

        if (validation.ProductType != CatalogSellableItemType.Simple)
            throw new InvalidPriceTargetException("Product-level pricing is allowed only for simple products.");

        var price = Money.Create(command.Amount, priceList.Currency);
        var compareAt = command.CompareAtAmount.HasValue
            ? Money.Create(command.CompareAtAmount.Value, priceList.Currency)
            : null;

        priceList.SetProductPrice(command.ProductId, price, compareAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
