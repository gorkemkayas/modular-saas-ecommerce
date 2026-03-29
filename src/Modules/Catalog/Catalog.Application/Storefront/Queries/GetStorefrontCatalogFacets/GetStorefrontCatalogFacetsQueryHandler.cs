using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontCatalogFacets
{
    public sealed class GetStorefrontCatalogFacetsQueryHandler
        : IRequestHandler<GetStorefrontCatalogFacetsQuery, StorefrontCatalogFacetsDto>
    {
        private readonly IStorefrontCatalogReadService _storefrontCatalogReadService;

        public GetStorefrontCatalogFacetsQueryHandler(IStorefrontCatalogReadService storefrontCatalogReadService)
        {
            _storefrontCatalogReadService = storefrontCatalogReadService;
        }

        public Task<StorefrontCatalogFacetsDto> Handle(
            GetStorefrontCatalogFacetsQuery query,
            CancellationToken cancellationToken)
        {
            var criteria = new StorefrontCatalogFacetCriteria(
                query.StoreId,
                query.SearchTerm,
                query.CategoryId,
                query.BrandId);

            return _storefrontCatalogReadService.GetFacetsAsync(criteria, cancellationToken);
        }
    }
}
