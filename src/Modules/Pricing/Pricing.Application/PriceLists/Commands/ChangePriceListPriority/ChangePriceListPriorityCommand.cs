using MediatR;

namespace Pricing.Application.PriceLists.Commands.ChangePriceListPriority;

public sealed record ChangePriceListPriorityCommand(
    Guid StoreId,
    Guid PriceListId,
    int Priority) : IRequest;
