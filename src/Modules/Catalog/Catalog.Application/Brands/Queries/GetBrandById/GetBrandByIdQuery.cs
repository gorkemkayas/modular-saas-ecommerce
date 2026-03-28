using Catalog.Application.Brands.DTOs;
using MediatR;

namespace Catalog.Application.Brands.Queries.GetBrandById
{
    public sealed record GetBrandByIdQuery(Guid StoreId, Guid BrandId) : IRequest<BrandDto?>;
}
