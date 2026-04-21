using MediatR;

namespace Pricing.Application.PriceLists.Commands.SetProductPrice;

public sealed record SetProductPriceCommand(
    Guid StoreId,
    Guid PriceListId,
    Guid ProductId,
    decimal Amount,
    decimal? CompareAtAmount) : IRequest;
