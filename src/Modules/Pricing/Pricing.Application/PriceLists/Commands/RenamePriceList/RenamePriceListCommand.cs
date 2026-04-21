using MediatR;

namespace Pricing.Application.PriceLists.Commands.RenamePriceList;

public sealed record RenamePriceListCommand(
    Guid StoreId,
    Guid PriceListId,
    string Name) : IRequest;
