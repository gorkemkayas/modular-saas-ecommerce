using MediatR;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Repositories;

namespace Pricing.Application.PriceLists.Commands.DeactivatePriceList;

public sealed class DeactivatePriceListCommandHandler : IRequestHandler<DeactivatePriceListCommand>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePriceListCommandHandler(IPriceListRepository priceListRepository, IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivatePriceListCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.StoreId, command.PriceListId, cancellationToken)
            ?? throw new PriceListNotFoundException(command.PriceListId);

        priceList.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
