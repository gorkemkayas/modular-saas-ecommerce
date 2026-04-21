using MediatR;

namespace Pricing.Application.PriceLists.Commands.ActivatePriceEntry;

public sealed record ActivatePriceEntryCommand(
    Guid StoreId,
    Guid PriceListId,
    Guid PriceEntryId) : IRequest;
