using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Categories.DTOs;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryById
{
    public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
    {
        private readonly ICategoryReadService _categoryReadService;

        public GetCategoryByIdQueryHandler(ICategoryReadService categoryReadService)
        {
            _categoryReadService = categoryReadService;
        }

        public Task<CategoryDto?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            return _categoryReadService.GetByIdAsync(query.StoreId, query.CategoryId, cancellationToken);
        }
    }
}
