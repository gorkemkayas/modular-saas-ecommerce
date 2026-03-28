using MediatR;

namespace Catalog.Application.Categories.Commands.ActivateCategory
{
    public sealed record ActivateCategoryCommand(Guid StoreId, Guid CategoryId) : IRequest;
}
