using MediatR;

namespace Catalog.Application.Products.Commands.CreateVariantProduct
{
    public sealed record CreateVariantProductCommand(
        Guid StoreId,
        string Name,
        string Slug,
        string? ShortDescription,
        string? Description,
        Guid? BrandId,
        IReadOnlyCollection<Guid>? CategoryIds) : IRequest<Guid>;
}
