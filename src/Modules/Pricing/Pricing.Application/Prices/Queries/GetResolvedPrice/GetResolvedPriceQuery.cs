using MediatR;
using Pricing.Application.Prices.DTOs;

namespace Pricing.Application.Prices.Queries.GetResolvedPrice;

public sealed record GetResolvedPriceQuery(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    string CurrencyCode) : IRequest<ResolvedPriceDto?>;
