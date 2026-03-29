using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontCategoryTree
{
    public sealed class GetStorefrontCategoryTreeQueryHandler
        : IRequestHandler<GetStorefrontCategoryTreeQuery, IReadOnlyCollection<StorefrontCategoryTreeNodeDto>>
    {
        private readonly IStorefrontCatalogReadService _storefrontCatalogReadService;

        public GetStorefrontCategoryTreeQueryHandler(IStorefrontCatalogReadService storefrontCatalogReadService)
        {
            _storefrontCatalogReadService = storefrontCatalogReadService;
        }

        public Task<IReadOnlyCollection<StorefrontCategoryTreeNodeDto>> Handle(
            GetStorefrontCategoryTreeQuery query,
            CancellationToken cancellationToken)
        {
            return _storefrontCatalogReadService.GetCategoryTreeAsync(query.StoreId, cancellationToken);
        }
    }
}
