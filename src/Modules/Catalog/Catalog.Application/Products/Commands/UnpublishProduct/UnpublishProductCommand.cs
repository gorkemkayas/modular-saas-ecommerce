using MediatR;

namespace Catalog.Application.Products.Commands.UnpublishProduct
{
    public sealed record UnpublishProductCommand(Guid StoreId, Guid ProductId) : IRequest;
}
