using Catalog.Application.Products.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(Guid StoreId, Guid ProductId) : IRequest<ProductDto?>;
}
