using MediatR;

namespace Catalog.Application.Products.Commands.CreateSimpleProduct
{
    public sealed record CreateSimpleProductCommand(
        Guid StoreId,
        string Name,
        string Slug,
        string Sku,
        string? ShortDescription,
        string? Description,
        Guid? BrandId,
        IReadOnlyCollection<Guid>? CategoryIds) : IRequest<Guid>;
}
