using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.SearchStorefrontBrands
{
    public sealed class SearchStorefrontBrandsQueryHandler
        : IRequestHandler<SearchStorefrontBrandsQuery, IReadOnlyCollection<StorefrontBrandDto>>
    {
        private readonly IStorefrontCatalogReadService _storefrontCatalogReadService;

        public SearchStorefrontBrandsQueryHandler(IStorefrontCatalogReadService storefrontCatalogReadService)
        {
            _storefrontCatalogReadService = storefrontCatalogReadService;
        }

        public Task<IReadOnlyCollection<StorefrontBrandDto>> Handle(
            SearchStorefrontBrandsQuery query,
            CancellationToken cancellationToken)
        {
            return _storefrontCatalogReadService.SearchBrandsAsync(
                query.StoreId,
                query.SearchTerm,
                cancellationToken);
        }
    }
}
