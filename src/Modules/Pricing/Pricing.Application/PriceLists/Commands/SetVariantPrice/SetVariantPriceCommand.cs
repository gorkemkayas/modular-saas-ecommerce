using MediatR;

namespace Pricing.Application.PriceLists.Commands.SetVariantPrice;

public sealed record SetVariantPriceCommand(
    Guid StoreId,
    Guid PriceListId,
    Guid ProductId,
    Guid ProductVariantId,
    decimal Amount,
    decimal? CompareAtAmount) : IRequest;
