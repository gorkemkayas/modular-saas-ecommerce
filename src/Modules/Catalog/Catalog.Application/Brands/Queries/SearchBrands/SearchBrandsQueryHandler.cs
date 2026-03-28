using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Brands.DTOs;
using MediatR;

namespace Catalog.Application.Brands.Queries.SearchBrands
{
    public sealed class SearchBrandsQueryHandler : IRequestHandler<SearchBrandsQuery, IReadOnlyCollection<BrandDto>>
    {
        private readonly IBrandReadService _brandReadService;

        public SearchBrandsQueryHandler(IBrandReadService brandReadService)
        {
            _brandReadService = brandReadService;
        }

        public Task<IReadOnlyCollection<BrandDto>> Handle(SearchBrandsQuery query, CancellationToken cancellationToken)
        {
            return _brandReadService.SearchAsync(query.StoreId, query.SearchTerm, query.ActiveOnly, cancellationToken);
        }
    }
}
