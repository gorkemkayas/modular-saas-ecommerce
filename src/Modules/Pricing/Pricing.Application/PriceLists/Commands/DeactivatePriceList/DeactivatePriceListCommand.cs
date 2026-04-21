using MediatR;

namespace Pricing.Application.PriceLists.Commands.DeactivatePriceList;

public sealed record DeactivatePriceListCommand(Guid StoreId, Guid PriceListId) : IRequest;
