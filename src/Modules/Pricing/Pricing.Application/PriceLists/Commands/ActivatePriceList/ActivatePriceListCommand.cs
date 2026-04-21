using MediatR;

namespace Pricing.Application.PriceLists.Commands.ActivatePriceList;

public sealed record ActivatePriceListCommand(Guid StoreId, Guid PriceListId) : IRequest;
