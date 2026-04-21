using MediatR;

namespace Pricing.Application.PriceLists.Commands.DeactivatePriceEntry;

public sealed record DeactivatePriceEntryCommand(
    Guid StoreId,
    Guid PriceListId,
    Guid PriceEntryId) : IRequest;
