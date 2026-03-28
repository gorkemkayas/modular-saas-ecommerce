using MediatR;

namespace Catalog.Application.Products.Commands.ActivateProduct
{
    public sealed record ActivateProductCommand(Guid StoreId, Guid ProductId) : IRequest;
}
