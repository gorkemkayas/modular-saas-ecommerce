using Catalog.Application.Categories.DTOs;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryById
{
    public sealed record GetCategoryByIdQuery(Guid StoreId, Guid CategoryId) : IRequest<CategoryDto?>;
}
