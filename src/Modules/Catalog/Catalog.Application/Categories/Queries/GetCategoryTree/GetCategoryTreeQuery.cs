using Catalog.Application.Categories.DTOs;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryTree
{
    public sealed record GetCategoryTreeQuery(Guid StoreId) : IRequest<IReadOnlyCollection<CategoryTreeNodeDto>>;
}
