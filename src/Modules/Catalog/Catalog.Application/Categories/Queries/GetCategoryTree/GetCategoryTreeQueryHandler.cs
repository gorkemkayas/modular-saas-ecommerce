using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Categories.DTOs;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryTree
{
    public sealed class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, IReadOnlyCollection<CategoryTreeNodeDto>>
    {
        private readonly ICategoryReadService _categoryReadService;

        public GetCategoryTreeQueryHandler(ICategoryReadService categoryReadService)
        {
            _categoryReadService = categoryReadService;
        }

        public Task<IReadOnlyCollection<CategoryTreeNodeDto>> Handle(GetCategoryTreeQuery query, CancellationToken cancellationToken)
        {
            return _categoryReadService.GetTreeAsync(query.StoreId, cancellationToken);
        }
    }
}
