using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.ActivatePriceEntry;

public sealed class ActivatePriceEntryCommandHandler : IRequestHandler<ActivatePriceEntryCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePriceEntryCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivatePriceEntryCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        priceList.ActivatePriceEntry(command.PriceEntryId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
