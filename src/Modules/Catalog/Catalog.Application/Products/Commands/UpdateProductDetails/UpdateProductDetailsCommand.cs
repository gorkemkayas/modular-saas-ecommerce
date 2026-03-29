using MediatR;

namespace Catalog.Application.Products.Commands.UpdateProductDetails
{
    public sealed record UpdateProductDetailsCommand(
        Guid StoreId,
        Guid ProductId,
        string Name,
        string? ShortDescription,
        string? Description,
        Guid? BrandId) : IRequest;
}
