using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.ChangePriceListPriority;

public sealed class ChangePriceListPriorityCommandHandler : IRequestHandler<ChangePriceListPriorityCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePriceListPriorityCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangePriceListPriorityCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        priceList.ChangePriority(command.Priority);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
