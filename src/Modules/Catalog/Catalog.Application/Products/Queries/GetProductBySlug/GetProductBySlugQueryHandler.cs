using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Products.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductBySlug
{
    public sealed class GetProductBySlugQueryHandler : IRequestHandler<GetProductBySlugQuery, ProductDto?>
    {
        private readonly IProductReadService _productReadService;

        public GetProductBySlugQueryHandler(IProductReadService productReadService)
        {
            _productReadService = productReadService;
        }

        public Task<ProductDto?> Handle(GetProductBySlugQuery query, CancellationToken cancellationToken)
        {
            return _productReadService.GetBySlugAsync(query.StoreId, query.Slug, cancellationToken);
        }
    }
}
