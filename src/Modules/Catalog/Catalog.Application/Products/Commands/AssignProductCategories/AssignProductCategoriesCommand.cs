using MediatR;

namespace Catalog.Application.Products.Commands.AssignProductCategories
{
    public sealed record AssignProductCategoriesCommand(
        Guid StoreId,
        Guid ProductId,
        IReadOnlyCollection<Guid> CategoryIds) : IRequest;
}
