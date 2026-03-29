using MediatR;

namespace Catalog.Application.Products.Commands.PublishProduct
{
    public sealed record PublishProductCommand(Guid StoreId, Guid ProductId) : IRequest;
}
