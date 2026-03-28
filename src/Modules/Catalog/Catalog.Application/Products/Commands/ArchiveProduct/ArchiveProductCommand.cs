using MediatR;

namespace Catalog.Application.Products.Commands.ArchiveProduct
{
    public sealed record ArchiveProductCommand(Guid StoreId, Guid ProductId) : IRequest;
}
