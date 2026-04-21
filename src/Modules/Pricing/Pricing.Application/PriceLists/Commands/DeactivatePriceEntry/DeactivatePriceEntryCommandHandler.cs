using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.DeactivatePriceEntry;

public sealed class DeactivatePriceEntryCommandHandler : IRequestHandler<DeactivatePriceEntryCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePriceEntryCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivatePriceEntryCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        priceList.DeactivatePriceEntry(command.PriceEntryId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
