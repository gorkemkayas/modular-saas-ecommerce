using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontProductBySlug
{
    public sealed class GetStorefrontProductBySlugQueryHandler
        : IRequestHandler<GetStorefrontProductBySlugQuery, StorefrontProductDto?>
    {
        private readonly IStorefrontCatalogReadService _storefrontCatalogReadService;

        public GetStorefrontProductBySlugQueryHandler(IStorefrontCatalogReadService storefrontCatalogReadService)
        {
            _storefrontCatalogReadService = storefrontCatalogReadService;
        }

        public Task<StorefrontProductDto?> Handle(
            GetStorefrontProductBySlugQuery query,
            CancellationToken cancellationToken)
        {
            return _storefrontCatalogReadService.GetProductBySlugAsync(
                query.StoreId,
                query.Slug,
                cancellationToken);
        }
    }
}
