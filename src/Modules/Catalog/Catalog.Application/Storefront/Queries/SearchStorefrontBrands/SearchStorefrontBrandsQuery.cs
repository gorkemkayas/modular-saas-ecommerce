using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.SearchStorefrontBrands
{
    public sealed record SearchStorefrontBrandsQuery(
        Guid StoreId,
        string? SearchTerm) : IRequest<IReadOnlyCollection<StorefrontBrandDto>>;
}
