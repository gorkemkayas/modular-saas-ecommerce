using MediatR;

namespace Catalog.Application.Categories.Commands.ChangeCategoryParent
{
    public sealed record ChangeCategoryParentCommand(
        Guid StoreId,
        Guid CategoryId,
        Guid? ParentCategoryId) : IRequest;
}
