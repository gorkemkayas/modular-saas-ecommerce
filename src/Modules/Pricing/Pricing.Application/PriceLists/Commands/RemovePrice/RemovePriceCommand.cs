using MediatR;

namespace Pricing.Application.PriceLists.Commands.RemovePrice;

public sealed record RemovePriceCommand(
    Guid StoreId,
    Guid PriceListId,
    Guid ProductId,
    Guid? ProductVariantId) : IRequest;
