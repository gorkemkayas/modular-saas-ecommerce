using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Products.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductReadService _productReadService;

        public GetProductByIdQueryHandler(IProductReadService productReadService)
        {
            _productReadService = productReadService;
        }

        public Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            return _productReadService.GetByIdAsync(query.StoreId, query.ProductId, cancellationToken);
        }
    }
}
