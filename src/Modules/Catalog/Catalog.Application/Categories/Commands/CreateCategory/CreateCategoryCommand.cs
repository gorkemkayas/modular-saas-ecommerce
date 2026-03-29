using MediatR;

namespace Catalog.Application.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(
        Guid StoreId,
        string Name,
        string Slug,
        string? Description,
        Guid? ParentCategoryId,
        int SortOrder) : IRequest<Guid>;
}
