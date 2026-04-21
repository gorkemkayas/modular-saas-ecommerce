using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.ActivatePriceList;

public sealed class ActivatePriceListCommandHandler : IRequestHandler<ActivatePriceListCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePriceListCommandHandler(IPriceListRepository priceListRepository, IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivatePriceListCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        if (priceList.IsDefault &&
            await _priceListRepository.ExistsDefaultActiveListAsync(
                command.StoreId,
                priceList.Currency,
                excludedPriceListId: priceList.Id,
                cancellationToken: cancellationToken))
        {
            throw new DuplicateDefaultPriceListException(command.StoreId, priceList.Currency.Code);
        }

        priceList.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
