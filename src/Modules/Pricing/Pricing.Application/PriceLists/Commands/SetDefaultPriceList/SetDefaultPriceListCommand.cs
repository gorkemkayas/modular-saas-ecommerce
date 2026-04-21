using MediatR;

namespace Pricing.Application.PriceLists.Commands.SetDefaultPriceList;

public sealed record SetDefaultPriceListCommand(Guid StoreId, Guid PriceListId) : IRequest;
