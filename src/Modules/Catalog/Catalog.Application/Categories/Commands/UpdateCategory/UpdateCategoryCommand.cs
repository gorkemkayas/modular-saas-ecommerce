using MediatR;

namespace Catalog.Application.Categories.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommand(
        Guid StoreId,
        Guid CategoryId,
        string Name,
        string Slug,
        string? Description,
        int SortOrder) : IRequest;
}
