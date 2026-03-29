using Catalog.Application.Products.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductBySlug
{
    public sealed record GetProductBySlugQuery(Guid StoreId, string Slug) : IRequest<ProductDto?>;
}
