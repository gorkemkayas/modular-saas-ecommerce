using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontProductBySlug
{
    public sealed record GetStorefrontProductBySlugQuery(
        Guid StoreId,
        string Slug,
        string CurrencyCode = "TRY") : IRequest<StorefrontProductDto?>;
}
