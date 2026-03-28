using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Brands.DTOs;
using MediatR;

namespace Catalog.Application.Brands.Queries.GetBrandById
{
    public sealed class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto?>
    {
        private readonly IBrandReadService _brandReadService;

        public GetBrandByIdQueryHandler(IBrandReadService brandReadService)
        {
            _brandReadService = brandReadService;
        }

        public Task<BrandDto?> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
        {
            return _brandReadService.GetByIdAsync(query.StoreId, query.BrandId, cancellationToken);
        }
    }
}
