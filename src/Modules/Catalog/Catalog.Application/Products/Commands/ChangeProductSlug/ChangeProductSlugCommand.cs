using MediatR;

namespace Catalog.Application.Products.Commands.ChangeProductSlug
{
    public sealed record ChangeProductSlugCommand(
        Guid StoreId,
        Guid ProductId,
        string Slug) : IRequest;
}
