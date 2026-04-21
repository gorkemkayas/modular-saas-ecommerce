using MediatR;
using Pricing.Application.Common.Models;
using Pricing.Application.PriceLists.DTOs;
using Pricing.Domain.Enums;

namespace Pricing.Application.PriceLists.Queries.SearchPriceLists;

public sealed record SearchPriceListsQuery(
    Guid StoreId,
    string? CurrencyCode,
    PriceListStatus? Status,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<PriceListSummaryDto>>;
