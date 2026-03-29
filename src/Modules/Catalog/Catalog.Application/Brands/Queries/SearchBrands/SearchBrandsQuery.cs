using Catalog.Application.Brands.DTOs;
using MediatR;

namespace Catalog.Application.Brands.Queries.SearchBrands
{
    public sealed record SearchBrandsQuery(Guid StoreId, string? SearchTerm, bool ActiveOnly = false) : IRequest<IReadOnlyCollection<BrandDto>>;
}
