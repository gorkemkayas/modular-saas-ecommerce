using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.SetDefaultPriceList;

public sealed class SetDefaultPriceListCommandHandler : IRequestHandler<SetDefaultPriceListCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultPriceListCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetDefaultPriceListCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        var existingDefault = await _priceListRepository.GetDefaultByStoreAndCurrencyAsync(
            command.StoreId,
            priceList.Currency,
            cancellationToken);

        if (existingDefault is not null && existingDefault.Id != priceList.Id)
            existingDefault.UnmarkAsDefault();

        priceList.MarkAsDefault();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
