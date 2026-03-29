using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontCatalogFacets
{
    public sealed record GetStorefrontCatalogFacetsQuery(
        Guid StoreId,
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId) : IRequest<StorefrontCatalogFacetsDto>;
}
