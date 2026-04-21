using MediatR;
using Pricing.Application.PriceLists.DTOs;

namespace Pricing.Application.PriceLists.Queries.GetPriceListById;

public sealed record GetPriceListByIdQuery(Guid StoreId, Guid PriceListId) : IRequest<PriceListDto?>;
