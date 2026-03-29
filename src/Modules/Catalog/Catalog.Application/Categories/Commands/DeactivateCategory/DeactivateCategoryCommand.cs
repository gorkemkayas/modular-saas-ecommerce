using MediatR;

namespace Catalog.Application.Categories.Commands.DeactivateCategory
{
    public sealed record DeactivateCategoryCommand(Guid StoreId, Guid CategoryId) : IRequest;
}
