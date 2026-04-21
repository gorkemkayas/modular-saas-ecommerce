using MediatR;

namespace Pricing.Application.PriceLists.Commands.CreatePriceList;

public sealed record CreatePriceListCommand(
    Guid StoreId,
    string Name,
    string CurrencyCode,
    int Priority,
    bool IsDefault) : IRequest<Guid>;
